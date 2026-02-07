using ApplicationCore.UseCases.Orders.CreateOrder;

namespace eShopX.Endpoints.Orders;

public class CreateOrderEndpoint : IGroupedEndpoint<OrdersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("", Handle)
            .Accepts<CreateOrderCommand>(MediaTypeNames.Application.Json)
            .Produces<CreateOrderResponse>()
            .WithDescription("建立訂單");
    }

    private static async Task<IResult> Handle([FromBody] CreateOrderCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Created(
            $"/api/orders/{result.OrderId}",
            ApiResponse<CreateOrderResponse>.Success(result, "訂單建立成功", StatusCodes.Status201Created));
    }
}
