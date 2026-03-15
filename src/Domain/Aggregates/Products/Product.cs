using eShopX.Domain.Aggregates.Products.Events;

namespace eShopX.Domain.Aggregates.Products;

public sealed class Product : AggregateRoot
{
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductTag> _tags = [];

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Audience? Audience { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<ProductVariant> Variants => _variants;
    public IReadOnlyList<ProductTag> Tags => _tags;

    private Product() { }

    public static Product Create(string name, string? description, Audience? audience, Guid categoryId)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Audience = audience,
            IsActive = false,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id));

        return product;
    }

    public ProductVariant AddVariant(string color)
    {
        var variant = ProductVariant.Create(Id, color);
        _variants.Add(variant);
        return variant;
    }

    public void AddTag(Guid tagId)
    {
        var tag = ProductTag.Create(Id, tagId);
        _tags.Add(tag);
    }

    public void SyncTags(IReadOnlyList<Guid> tagIds)
    {
        _tags.Clear();
        foreach (var tagId in tagIds)
            AddTag(tagId);
    }

    public void RemoveVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is not null)
            _variants.Remove(variant);
    }

    public void Update(string name, string? description, Audience? audience, Guid categoryId)
    {
        Name = name;
        Description = description;
        Audience = audience;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductUpdatedEvent(Id));
    }

    public void Publish()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductUpdatedEvent(Id));
    }

    public void Unpublish()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductUpdatedEvent(Id));
    }

    public void Delete()
    {
        RaiseDomainEvent(new ProductDeletedEvent(Id));
    }
}
