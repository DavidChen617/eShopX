namespace ApplicationCore.UseCases.Orders.GetOrder;

public record GetOrderQuery(Guid OrderId) : IRequest<GetOrderResponse>;