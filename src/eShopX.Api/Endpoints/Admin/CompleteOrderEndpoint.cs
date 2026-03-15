using eShopX.Application.UseCases.Orders;

namespace eShopX.Endpoints.Admin;

public sealed class CompleteOrderEndpoint : IGroupedEndpoint<AdminOrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{orderId:guid}/complete", Handle);
    }

    private static async Task<IResult> Handle(
        Guid orderId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new MarkOrderAsCompletedCommand(orderId), ct);
        return result.ToHttpResult();
    }
}
