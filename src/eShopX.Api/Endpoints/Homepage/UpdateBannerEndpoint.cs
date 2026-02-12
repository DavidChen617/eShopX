using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Homepage.UpdateBanner;

namespace eShopX.Endpoints.Homepage;

public class UpdateBannerEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/banners/{id:guid}", Handle)
            .Accepts<UpdateBannerRequest>(MediaTypeNames.Multipart.FormData)
            .Produces<UpdateBannerResponse>()
            .DisableAntiforgery()
            .WithDescription("更新 Banner")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromForm] UpdateBannerRequest request,
        ISender sender)
    {
        await using var stream = request.File?.OpenReadStream();
        var imageRequest = stream is null || request.File is null
            ? null
            : new ImageUploadRequest(
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length);

        var command = new UpdateBannerCommand(
            id,
            request.Title,
            request.ImageUrl,
            imageRequest,
            request.Link,
            request.SortOrder,
            request.IsActive,
            request.StartsAt,
            request.EndsAt);
        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<UpdateBannerResponse>.Success(result));
    }
}

public record UpdateBannerRequest(
    string Title,
    string? ImageUrl,
    IFormFile? File,
    string Link,
    int SortOrder,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? EndsAt);
