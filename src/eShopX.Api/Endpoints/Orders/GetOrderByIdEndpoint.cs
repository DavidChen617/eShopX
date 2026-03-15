using eShopX.Application.UseCases.Orders;

namespace eShopX.Endpoints.Orders;

public sealed class GetOrderByIdEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{orderId:guid}", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid orderId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetOrderByIdQuery(orderId), ct);
        return result.ToHttpResult();
    }
}
