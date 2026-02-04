namespace ApplicationCore.UseCases.Orders.CancelOrder;

public record CancelOrderResponse(Guid OrderId, OrderStatus Status, DateTime CancelledAt);
