namespace eShopX.Common.Mapping;

public abstract class Profile
{
    private readonly List<MappingDefinition> _maps = new();

    protected MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var definition = new MappingDefinition(typeof(TSource), typeof(TDestination));
        _maps.Add(definition);
        return new MappingExpression<TSource, TDestination>(this, definition);
    }

    internal IReadOnlyList<MappingDefinition> GetMaps() => _maps;

    internal MappingDefinition AddReverse(Type source, Type destination)
    {
        var definition = new MappingDefinition(source, destination);
        _maps.Add(definition);
        return definition;
    }
}

internal readonly record struct TypePair(Type Source, Type Destination);