namespace ApplicationCore.UseCases.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus Status) : IRequest<UpdateOrderStatusResponse>;
