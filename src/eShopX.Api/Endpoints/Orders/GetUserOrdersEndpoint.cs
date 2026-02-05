using ApplicationCore.UseCases.Orders.GetUserOrders;

namespace eShopX.Endpoints.Orders;

public class GetUserOrdersEndpoint : IGroupedEndpoint<OrdersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("", Handle)
            .Produces<GetUserOrderResponse>()
            .WithDescription("查詢使用者的訂單列表，支援分頁");
    }

    private static async Task<IResult> Handle(
        [FromQuery] Guid userId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ISender sender)
    {
        var result = await sender.Send(new GetUserOrdersQuery(userId, page, pageSize));
        return Results.Ok(ApiResponse<GetUserOrderResponse>.Success(result));
    }
}