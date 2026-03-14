using eShopX.Domain.Aggregates.Orders.Events;
using eShopX.Domain.Exceptions;
using eShopX.Domain.ValueObjects;

namespace eShopX.Domain.Aggregates.Orders;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items;

    private Order() { }

    public static Order Create(Guid userId, IEnumerable<(Guid SkuId, ProductSnapshot Snapshot, Money UnitPrice, int Quantity)> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
            order._items.Add(OrderItem.Create(order.Id, item.SkuId, item.Snapshot, item.UnitPrice, item.Quantity));

        order.TotalAmount = order._items.Aggregate(Money.Of(0), (sum, i) => sum + i.TotalPrice);

        return order;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new ArgumentInvalidException("Only pending payment orders can be marked as paid.");
        Status = OrderStatus.Paid;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
            throw new ArgumentInvalidException("Only paid orders can be marked as shipped.");
        Status = OrderStatus.Shipped;
        RaiseDomainEvent(new OrderShippedEvent(Id, UserId));
    }

    public void MarkAsCompleted()
    {
        if (Status != OrderStatus.Shipped)
            throw new ArgumentInvalidException("Only shipped orders can be marked as completed.");
        Status = OrderStatus.Completed;
    }
}
