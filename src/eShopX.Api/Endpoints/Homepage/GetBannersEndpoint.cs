using ApplicationCore.UseCases.Homepage.GetBanners;

namespace eShopX.Endpoints.Homepage;

public class GetBannersEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/banners", Handle)
            .Produces<GetBannersResponse>()
            .WithDescription("取得首頁輪播 Banner");
    }

    private static async Task<IResult> Handle(ISender sender)
    {
        var result = await sender.Send(new GetBannersQuery());
        return Results.Ok(ApiResponse<GetBannersResponse>.Success(result));
    }
}
