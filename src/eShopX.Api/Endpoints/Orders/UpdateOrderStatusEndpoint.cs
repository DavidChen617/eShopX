using ApplicationCore.UseCases.Orders;
using ApplicationCore.UseCases.Orders.UpdateOrderStatus;

namespace eShopX.Endpoints.Orders;

public class UpdateOrderStatusEndpoint : IGroupedEndpoint<OrdersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPatch("{id}/status", Handle)
            .Accepts<UpdateOrderStatusRequest>(MediaTypeNames.Application.Json)
            .Produces<UpdateOrderStatusResponse>()
            .WithDescription("更新訂單狀態");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        ISender sender)
    {
        var result = await sender.Send(new UpdateOrderStatusCommand(id, request.Status));
        return Results.Ok(ApiResponse<UpdateOrderStatusResponse>.Success(result));
    }
}

public record UpdateOrderStatusRequest(OrderStatus Status);
