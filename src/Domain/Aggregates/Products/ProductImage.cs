namespace Domain.Aggregates.Products;

public sealed class ProductImage : Entity
{
    public Guid ProductVariantId { get; private set; }
    public string Url { get; private set; } = default!;
    public string PublicId { get; private set; } = default!;
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(Guid variantId, string url, string publicId, bool isPrimary, int sortOrder)
    {
        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductVariantId = variantId,
            Url = url,
            PublicId = publicId,
            IsPrimary = isPrimary,
            SortOrder = sortOrder
        };
    }
}
