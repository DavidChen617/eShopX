using System.Security.Claims;

using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Products.UploadProductImage;

namespace eShopX.Endpoints.Products;

public class UploadProductImageEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("{id}/images", Handle)
            .Accepts<UploadProductImageRequest>(MediaTypeNames.Multipart.FormData)
            .Produces<UploadProductImageResponse>()
            .DisableAntiforgery()
            .WithDescription("上傳商品圖片");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromRoute] Guid id,
        [FromForm] UploadProductImageRequest request,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        await using var stream = request.File.OpenReadStream();
        var imageRequest = new ImageUploadRequest(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.File.Length);

        var result = await sender.Send(new UploadProductImageCommand(
            userId,
            id,
            imageRequest,
            request.IsPrimary,
            request.SortOrder));

        return Results.Ok(ApiResponse<UploadProductImageResponse>.Success(result));
    }
}

public record UploadProductImageRequest(
    IFormFile File,
    bool IsPrimary,
    int SortOrder);
