using ApplicationCore.UseCases.Carts.AddCartItem;

namespace eShopX.Endpoints.Carts;

public class AddCartItemEndpoint : IGroupedEndpoint<CartGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{userId}/items", Handle)
            .Produces<ApiResponse<AddCartItemResponse>>(StatusCodes.Status201Created)
            .Accepts<AddCartItemRequest>(MediaTypeNames.Application.Json)
            .WithName("AddCartItem")
            .WithDescription("加入商品到購物車，若商品已存在則累加數量");
    }

    private static async Task<IResult> Handle([FromRoute] Guid userId, [FromBody] AddCartItemRequest request, ISender sender)
    {
        var command = new AddCartItemCommand(userId, request.ProductId, request.Quantity);
        var result = await sender.Send(command);

        return Results.Created($"/api/carts/{command.UserId}/items/{command.ProductId}",
            ApiResponse<AddCartItemResponse>.Success(result));
    }
}

public record AddCartItemRequest(Guid ProductId, int Quantity);
