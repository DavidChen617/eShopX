using ApplicationCore.UseCases.Carts.GetCart;

namespace eShopX.Endpoints.Carts;

public class GetCartEndpoint : IGroupedEndpoint<CartGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{id}", Handle)
            .Produces<GetCartResponse>()
            .WithName("GetCart")
            .WithDescription("取得使用者的購物車內容");
    }

    private static async Task<IResult> Handle([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new GetCartQuery(id));
        return Results.Ok(ApiResponse<GetCartResponse>.Success(result));
    }
}