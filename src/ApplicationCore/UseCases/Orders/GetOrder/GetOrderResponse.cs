namespace ApplicationCore.UseCases.Orders.GetOrder;

public record GetOrderResponse(
    Guid OrderId,
    Guid? UserId,
    OrderStatus? Status,
    decimal? TotalAmount,
    string? PaymentMethod,
    DateTime? PaidAt,
    string? ShippingName,
    string? ShippingAddress,
    string? ShippingPhone,
    DateTime? CreatedAt,
    List<QueryOrderItem>? Items);

public record QueryOrderItem(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal);