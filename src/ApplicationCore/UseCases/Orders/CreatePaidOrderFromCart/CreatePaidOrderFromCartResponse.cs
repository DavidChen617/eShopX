namespace ApplicationCore.UseCases.Orders.CreatePaidOrderFromCart;

public record CreatePaidOrderFromCartResponse(
    Guid OrderId,
    OrderStatus Status,
    decimal TotalAmount,
    string PaymentMethod,
    DateTime PaidAt);