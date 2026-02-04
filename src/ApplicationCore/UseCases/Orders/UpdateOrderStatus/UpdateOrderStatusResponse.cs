namespace ApplicationCore.UseCases.Orders.UpdateOrderStatus;

public record UpdateOrderStatusResponse(
    Guid OrderId,
    OrderStatus PreviousStatus,
    OrderStatus CurrentStatus,
    DateTime UpdatedAt);
