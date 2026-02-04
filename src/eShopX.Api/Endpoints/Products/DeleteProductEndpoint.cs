using System.Security.Claims;
using ApplicationCore.UseCases.Products.DeleteProduct;

namespace eShopX.Endpoints.Products;

public class DeleteProductEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("{id:guid}", Handle)
            .WithDescription("刪除商品");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromRoute] Guid id,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        await sender.Send(new DeleteProductCommand(userId, id));
        return Results.Ok(ApiResponse.NoContent("刪除成功"));
    }
}
