using System.Collections;
using System.Linq;

namespace eShopX.Common.Mapping;

public sealed class Mapper(MapperConfiguration config) : IMapper
{
    public TDestination Map<TDestination>(object source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sourceType = source.GetType();
        var destinationType = typeof(TDestination);

        if (!config.TryGetPlan(sourceType, destinationType, out var plan))
        {
            if (TryMapCollection(source, sourceType, destinationType, out var mapped))
                return (TDestination)mapped;

            throw new InvalidOperationException($"No map configured from {sourceType} to {destinationType}");
        }

        return (TDestination)plan!.MapObject(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!config.TryGetPlan<TSource, TDestination>(out var plan))
            throw new InvalidOperationException($"No map configured from {typeof(TSource)} to {typeof(TDestination)}");

        return plan!.Map(source);
    }

    private bool TryMapCollection(
        object source,
        Type sourceType,
        Type destinationType,
        out object mapped)
    {
        mapped = default!;

        if (!TryGetEnumerableElementType(sourceType, out var sourceElementType))
            return false;

        if (!TryGetEnumerableElementType(destinationType, out var destinationElementType))
            return false;

        if (!config.TryGetPlan(sourceElementType, destinationElementType, out var elementPlan))
            return false;

        var listType = typeof(List<>).MakeGenericType(destinationElementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var item in (IEnumerable)source)
        {
            if (item is null)
            {
                list.Add(destinationElementType.IsValueType
                    ? Activator.CreateInstance(destinationElementType)
                    : null);
                continue;
            }

            list.Add(elementPlan!.MapObject(item));
        }

        if (destinationType.IsArray)
        {
            var array = Array.CreateInstance(destinationElementType, list.Count);
            list.CopyTo(array, 0);
            mapped = array;
            return true;
        }

        if (destinationType.IsAssignableFrom(listType))
        {
            mapped = list;
            return true;
        }

        return false;
    }

    private static bool TryGetEnumerableElementType(Type type, out Type elementType)
    {
        elementType = default!;

        if (type == typeof(string))
            return false;

        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        var enumerable = type
            .GetInterfaces()
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerable is null)
            return false;

        elementType = enumerable.GetGenericArguments()[0];
        return true;
    }
}
