namespace ApplicationCore.UseCases.Orders.CancelOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest<CancelOrderResponse>;
