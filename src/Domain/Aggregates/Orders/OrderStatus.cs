namespace eShopX.Domain.Aggregates.Orders;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    Shipped,
    Completed
}
