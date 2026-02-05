using System.Security.Claims;

using ApplicationCore.UseCases.Products.UpdateProductImage;

namespace eShopX.Endpoints.Products;

public class UpdateProductImageEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("{productId}/images/{imageId}", Handle)
            .Accepts<UpdateProductImageRequest>(MediaTypeNames.Application.Json)
            .Produces<UpdateProductImageResponse>()
            .WithDescription("更新商品圖片（封面/排序）");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromRoute] Guid productId,
        [FromRoute] Guid imageId,
        [FromBody] UpdateProductImageRequest request,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new UpdateProductImageCommand(
            userId,
            productId,
            imageId,
            request.IsPrimary,
            request.SortOrder));
        return Results.Ok(ApiResponse<UpdateProductImageResponse>.Success(result));
    }
}

public record UpdateProductImageRequest(
    bool? IsPrimary,
    int? SortOrder);