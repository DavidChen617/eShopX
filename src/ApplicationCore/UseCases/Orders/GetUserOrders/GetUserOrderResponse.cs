namespace ApplicationCore.UseCases.Orders.GetUserOrders;

public record GetUserOrderResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    List<QueryUserOrderItem> Items
);

public record QueryUserOrderItem(
    Guid OrderId,
    OrderStatus OrderStatus,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAt,
    string PaymentMethod,
    DateTime? PaidAt
);
