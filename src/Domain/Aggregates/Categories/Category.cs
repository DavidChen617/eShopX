namespace eShopX.Domain.Aggregates.Categories;

public sealed class Category : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Category() { }

    public static Category Create(string name)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
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
