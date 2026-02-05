using ApplicationCore.UseCases.Carts.UpdateCartItemQuantity;

namespace eShopX.Endpoints.Carts;

public class UpdateCartItemEndpoint : IGroupedEndpoint<CartGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{userId}/items/{productId}", Handle)
            .Accepts<UpdateCartItemRequest>(MediaTypeNames.Application.Json)
            .Produces<UpdateCartItemResponse>()
            .WithName("UpdateCartItem")
            .WithDescription("更新購物車中商品的數量");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid userId,
        [FromRoute] Guid productId,
        [FromBody] UpdateCartItemRequest request,
        ISender sender)
    {
        var result = await sender.Send(new UpdateCartItemCommand(userId, productId, request.Quantity));
        return Results.Ok(ApiResponse<UpdateCartItemResponse>.Success(result));
    }
}

public record UpdateCartItemRequest(int Quantity);