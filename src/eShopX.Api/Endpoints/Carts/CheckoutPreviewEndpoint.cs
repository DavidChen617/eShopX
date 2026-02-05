using ApplicationCore.UseCases.Carts.CheckoutPreview;

namespace eShopX.Endpoints.Carts;

public class CheckoutPreviewEndpoint : IGroupedEndpoint<CartGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{userId}/checkout-preview", Handle)
            .Produces<ApiResponse<CheckoutPreviewResponse>>()
            .Accepts<Guid>(MediaTypeNames.Application.Json)
            .WithName("CheckoutPreview")
            .WithDescription("結帳預覽，檢查庫存並計算金額");
    }

    private static async Task<IResult> Handle([FromRoute] Guid userId, ISender sender)
    {
        var result = await sender.Send(new CheckoutPreviewQuery(userId));
        return Results.Ok(ApiResponse<CheckoutPreviewResponse>.Success(result));
    }
}