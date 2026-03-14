namespace eShopX.Domain.Aggregates.Products;

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

    public ProductSku AddSku(Guid? sizeId, decimal price, int stock)
    {
        var sku = ProductSku.Create(Id, sizeId, price, stock);
        _skus.Add(sku);
        return sku;
    }

    public void AddImage(string url, string publicId, bool isPrimary, int sortOrder)
    {
        var image = ProductImage.Create(Id, url, publicId, isPrimary, sortOrder);
        _images.Add(image);
    }
}
