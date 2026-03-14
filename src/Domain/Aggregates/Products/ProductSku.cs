using eShopX.Domain.Exceptions;

namespace eShopX.Domain.Aggregates.Products;

public sealed class ProductSku : Entity
{
    public Guid ProductVariantId { get; private set; }
    public Guid? SizeId { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }

    private ProductSku() { }

    public static ProductSku Create(Guid variantId, Guid? sizeId, decimal price, int stock)
    {
        if (price <= 0)
            throw new ArgumentInvalidException("Price must be greater than zero.");
        if (stock < 0)
            throw new ArgumentInvalidException("Stock quantity cannot be negative.");

        return new ProductSku
        {
            Id = Guid.NewGuid(),
            ProductVariantId = variantId,
            SizeId = sizeId,
            Price = price,
            StockQuantity = stock
        };
    }

    public void UpdatePrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentInvalidException("Price must be greater than zero.");
        Price = price;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentInvalidException("Quantity must be greater than zero.");
        StockQuantity += quantity;
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentInvalidException("Quantity must be greater than zero.");
        if (StockQuantity - quantity < 0)
            throw new ArgumentInvalidException("Insufficient stock.");
        StockQuantity -= quantity;
    }
}
