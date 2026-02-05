namespace eShopX.Endpoints.Auth;

public class AuthGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/api/auth";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Auth");
    }
}