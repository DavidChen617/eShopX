namespace ApplicationCore.UseCases.Orders.GetUserOrders;

public record GetUserOrdersQuery(Guid UserId, int Page, int PageSize) : IRequest<GetUserOrderResponse>;