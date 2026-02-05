using ApplicationCore.UseCases.Orders.GetOrder;

namespace eShopX.Endpoints.Orders;

public class GetOrderEndpoint : IGroupedEndpoint<OrdersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("{id}", Handle)
            .WithDescription("取得單一訂單詳細資訊");
    }

    private static async Task<IResult> Handle([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new GetOrderQuery(id));
        return Results.Ok(ApiResponse<GetOrderResponse>.Success(result));
    }
}