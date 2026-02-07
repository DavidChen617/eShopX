namespace eShopX.Common.Mapping;

public sealed class Mapper(MapperConfiguration config) : IMapper
{
    public TDestination Map<TDestination>(object source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sourceType = source.GetType();
        var destinationType = typeof(TDestination);

        if (!config.TryGetPlan(sourceType, destinationType, out var plan))
            throw new InvalidOperationException($"No map configured from {sourceType} to {destinationType}");

        return (TDestination)plan!.MapObject(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!config.TryGetPlan<TSource, TDestination>(out var plan))
            throw new InvalidOperationException($"No map configured from {typeof(TSource)} to {typeof(TDestination)}");

        return plan!.Map(source);
    }
}
