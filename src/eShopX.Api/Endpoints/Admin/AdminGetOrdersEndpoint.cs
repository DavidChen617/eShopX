using eShopX.Application.UseCases.Orders;
using eShopX.Domain.Aggregates.Orders;

namespace eShopX.Endpoints.Admin;

public sealed class AdminGetOrdersEndpoint : IGroupedEndpoint<AdminOrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle);
    }

    private static async Task<IResult> Handle(
        IDispatcher dispatcher,
        int page = 1,
        int pageSize = 20,
        OrderStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new GetOrdersQuery(null, status, page, pageSize), ct);
        return result.ToHttpResult();
    }
}
