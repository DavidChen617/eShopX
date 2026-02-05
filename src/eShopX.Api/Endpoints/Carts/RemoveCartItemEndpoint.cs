using ApplicationCore.UseCases.Carts.RemoveCartItem;

namespace eShopX.Endpoints.Carts;

public class RemoveCartItemEndpoint : IGroupedEndpoint<CartGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{userId}/items/{productId}", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .WithName("RemoveCartItem")
            .WithDescription("從購物車移除指定商品");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid userId,
        [FromRoute] Guid productId,
        ISender sender)
    {
        await sender.Send(new RemoveCartItemCommand(userId, productId));
        return Results.Ok(ApiResponse.NoContent("刪除成功"));
    }
}