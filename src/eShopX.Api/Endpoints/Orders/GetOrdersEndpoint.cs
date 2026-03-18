using Domain.Aggregates.Orders;

namespace eShopX.Endpoints.Orders;

public sealed class GetOrdersEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .Produces<ApiResponse<GetOrdersResponse>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        int page = 1,
        int pageSize = 20,
        OrderStatus? status = null,
        CancellationToken ct = default)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new GetOrdersQuery(userId, status, page, pageSize), ct);
        return result.ToHttpResult();
    }
}
