namespace ApplicationCore.UseCases.Orders.CreateOrder;

public record CreateOrderResponse(
    Guid OrderId,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime CreatedAt,
    string PaymentMethod,
    DateTime? PaidAt);
