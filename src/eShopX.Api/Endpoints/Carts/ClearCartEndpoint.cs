using ApplicationCore.UseCases.Carts.ClearCart;

namespace eShopX.Endpoints.Carts;

public class ClearCartEndpoint : IGroupedEndpoint<CartGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{userId}/items", Handle)
            .Produces(StatusCodes.Status204NoContent)
            .WithName("ClearCart")
            .WithDescription("清空購物車所有商品");
    }

    private static async Task<IResult> Handle([FromRoute] Guid userId, ISender sender)
    {
        await sender.Send(new ClearCartCommand(userId));
        return Results.Ok(ApiResponse.NoContent("清空成功"));
    }
}
