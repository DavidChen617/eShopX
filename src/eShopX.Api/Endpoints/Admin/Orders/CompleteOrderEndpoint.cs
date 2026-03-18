
namespace eShopX.Endpoints.Admin.Orders;

public sealed class CompleteOrderEndpoint : IGroupedEndpoint<AdminOrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{orderId:guid}/complete", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
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
