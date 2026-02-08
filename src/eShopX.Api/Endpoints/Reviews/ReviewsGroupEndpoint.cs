namespace eShopX.Endpoints.Reviews;

public class ReviewsGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix => "/api/reviews";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Reviews")
            .RequireAuthorization();
    }
}
