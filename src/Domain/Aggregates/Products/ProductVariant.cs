using Domain.ValueObjects;

namespace Domain.Aggregates.Products;

public sealed class ProductVariant : Entity
{
    private readonly List<ProductSku> _skus = [];
    private readonly List<ProductImage> _images = [];

    public Guid ProductId { get; private set; }
    public string Color { get; private set; } = default!;

    public IReadOnlyList<ProductSku> Skus => _skus;
    public IReadOnlyList<ProductImage> Images => _images;

    private ProductVariant() { }

    public static ProductVariant Create(Guid productId, string color)
    {
        return new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Color = color
        };
    }

    public ProductSku AddSku(Guid? sizeId, Money price, int stock)
    {
        var sku = ProductSku.Create(Id, sizeId, price, stock);
        _skus.Add(sku);
        return sku;
    }

    public void UpdateColor(string color) => Color = color;

    public void RemoveSku(Guid skuId)
    {
        var sku = _skus.FirstOrDefault(s => s.Id == skuId);
        if (sku is not null) _skus.Remove(sku);
    }

    public void AddImage(string url, string publicId, bool isPrimary, int sortOrder)
    {
        var image = ProductImage.Create(Id, url, publicId, isPrimary, sortOrder);
        _images.Add(image);
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is not null) _images.Remove(image);
    }
}
