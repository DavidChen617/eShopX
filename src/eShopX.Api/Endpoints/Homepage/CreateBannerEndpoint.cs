using ApplicationCore.UseCases.Homepage.CreateBanner;

namespace eShopX.Endpoints.Homepage;

public class CreateBannerEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/banners", Handle)
            .Accepts<CreateBannerCommand>(MediaTypeNames.Application.Json)
            .Produces<CreateBannerResponse>()
            .WithDescription("新增 Banner")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle([FromBody] CreateBannerCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<CreateBannerResponse>.Success(result));
    }
}
