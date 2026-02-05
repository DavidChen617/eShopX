namespace eShopX.Common.Mapping;

internal sealed class MappingDefinition(Type source, Type destination)
{
    private readonly List<CustomMemberMap> _customMemberMaps = new();
    private Func<object, object?>? _constructUsing;

    public TypePair TypePair { get; } = new(source, destination);

    public IReadOnlyList<CustomMemberMap> CustomMemberMaps => _customMemberMaps;
    public Func<object, object?>? ConstructUsing => _constructUsing;

    public void AddCustomMember(string destinationName, Func<object, object?> resolver, string? sourceName = null)
    {
        _customMemberMaps.Add(new CustomMemberMap(destinationName, sourceName, resolver));
    }

    public void AddCustomMember(string destinationName, string sourceName)
    {
        _customMemberMaps.Add(new CustomMemberMap(destinationName, sourceName, null));
    }

    public void SetConstructUsing(Func<object, object?> factory)
    {
        _constructUsing = factory;
    }
}

internal sealed record CustomMemberMap(string DestinationName, string? SourceName, Func<object, object?>? Resolver);