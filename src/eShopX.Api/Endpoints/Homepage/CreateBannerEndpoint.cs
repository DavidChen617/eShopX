using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Homepage.CreateBanner;

namespace eShopX.Endpoints.Homepage;

public class CreateBannerEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/banners", Handle)
            .Accepts<CreateBannerRequest>(MediaTypeNames.Multipart.FormData)
            .Produces<CreateBannerResponse>()
            .DisableAntiforgery()
            .WithDescription("新增 Banner")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle([FromForm] CreateBannerRequest request, ISender sender)
    {
        await using var stream = request.File?.OpenReadStream();
        var imageRequest = stream is null || request.File is null
            ? null
            : new ImageUploadRequest(
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length);

        var command = new CreateBannerCommand(
            request.Title,
            request.ImageUrl,
            imageRequest,
            request.Link,
            request.SortOrder,
            request.StartsAt,
            request.EndsAt);
        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<CreateBannerResponse>.Success(result));
    }
}

public record CreateBannerRequest(
    string Title,
    string? ImageUrl,
    IFormFile? File,
    string Link,
    int SortOrder,
    DateTime? StartsAt,
    DateTime? EndsAt);
