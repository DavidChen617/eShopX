using ApplicationCore.UseCases.Orders.CancelOrder;

namespace eShopX.Endpoints.Orders;

public class CancelOrderEndpoint : IGroupedEndpoint<OrdersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPatch("{id}/cancel", Handle)
            .Produces<CancelOrderResponse>()
            .WithDescription("取消訂單");
    }

    private static async Task<IResult> Handle([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new CancelOrderCommand(id));
        return Results.Ok(ApiResponse<CancelOrderResponse>.Success(result, "訂單取消成功"));
    }
}