using ApplicationCore.UseCases.Homepage.GetHomepageReviews;

namespace eShopX.Endpoints.Homepage;

public class GetHomepageReviewsEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/reviews", Handle)
            .Produces<ApiResponse<List<HomepageReviewItem>>>()
            .WithDescription("首頁精選評價");
    }

    private static async Task<IResult> Handle(
        [FromQuery] int limit,
        ISender sender)
    {
        var query = new GetHomepageReviewsQuery(limit < 1 ? 10 : limit);
        var result = await sender.Send(query);
        return Results.Ok(ApiResponse<List<HomepageReviewItem>>.Success(result));
    }
}
