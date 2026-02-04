using System.Security.Claims;
using ApplicationCore.UseCases.Products.DeleteProductImage;

namespace eShopX.Endpoints.Products;

public class DeleteProductImageEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("{productId}/images/{imageId}", Handle)
            .WithDescription("刪除商品圖片");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromRoute] Guid productId,
        [FromRoute] Guid imageId,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        await sender.Send(new DeleteProductImageCommand(userId, productId, imageId));
        return Results.Ok(ApiResponse.NoContent("刪除成功"));
    }
}
