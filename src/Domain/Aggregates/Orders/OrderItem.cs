namespace eShopX.Domain.Aggregates.Orders;

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid SkuId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string Color { get; private set; } = default!;
    public string? Size { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(Guid orderId, Guid skuId, string productName, string color, string? size, decimal unitPrice, int quantity)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            SkuId = skuId,
            ProductName = productName,
            Color = color,
            Size = size,
            UnitPrice = unitPrice,
            Quantity = quantity,
            TotalPrice = unitPrice * quantity
        };
    }
}
