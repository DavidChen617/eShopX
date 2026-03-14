namespace eShopX.Domain.Aggregates.Products;

public sealed class ProductTag
{
    public Guid ProductId { get; private set; }
    public Guid TagId { get; private set; }

    private ProductTag() { }

    public static ProductTag Create(Guid productId, Guid tagId)
    {
        return new ProductTag
        {
            ProductId = productId,
            TagId = tagId
        };
    }
}
