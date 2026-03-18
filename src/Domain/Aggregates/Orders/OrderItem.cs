using Domain.ValueObjects;

namespace Domain.Aggregates.Orders;

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid SkuId { get; private set; }
    public ProductSnapshot Snapshot { get; private set; } = default!;
    public Money UnitPrice { get; private set; } = default!;
    public int Quantity { get; private set; }
    public Money TotalPrice { get; private set; } = default!;

    private OrderItem() { }

    public static OrderItem Create(Guid orderId, Guid skuId, ProductSnapshot snapshot, Money unitPrice, int quantity)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            SkuId = skuId,
            Snapshot = snapshot,
            UnitPrice = unitPrice,
            Quantity = quantity,
            TotalPrice = unitPrice * quantity
        };
    }
}
