using ApplicationCore.UseCases.Homepage.DeleteBanner;

namespace eShopX.Endpoints.Homepage;

public class DeleteBannerEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/banners/{id:guid}", Handle)
            .Produces<DeleteBannerResponse>()
            .WithDescription("刪除 Banner")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteBannerCommand(id));
        return Results.Ok(ApiResponse<DeleteBannerResponse>.Success(result));
    }
}