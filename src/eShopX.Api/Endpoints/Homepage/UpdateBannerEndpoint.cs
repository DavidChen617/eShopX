using ApplicationCore.UseCases.Homepage.UpdateBanner;

namespace eShopX.Endpoints.Homepage;

public class UpdateBannerEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/banners/{id:guid}", Handle)
            .Accepts<UpdateBannerRequest>(MediaTypeNames.Application.Json)
            .Produces<UpdateBannerResponse>()
            .WithDescription("更新 Banner")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateBannerRequest request,
        ISender sender)
    {
        var command = new UpdateBannerCommand(
            id,
            request.Title,
            request.ImageUrl,
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
    string ImageUrl,
    string Link,
    int SortOrder,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? EndsAt);
