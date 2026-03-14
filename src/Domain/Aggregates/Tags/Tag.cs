namespace eShopX.Domain.Aggregates.Tags;

public sealed class Tag : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public TagType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Tag() { }

    public static Tag Create(string name, TagType type)
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Rename(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
