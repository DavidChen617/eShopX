namespace Domain.Aggregates.Sizes;

public sealed class Size : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public SizeType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Size() { }

    public static Size Create(string name, SizeType type)
    {
        return new Size
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
