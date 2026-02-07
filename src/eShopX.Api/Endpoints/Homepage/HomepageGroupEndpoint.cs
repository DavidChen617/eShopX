namespace eShopX.Endpoints.Homepage;

public class HomepageGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix => "/api/homepage";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Homepage")
            .AllowAnonymous();
    }
}
