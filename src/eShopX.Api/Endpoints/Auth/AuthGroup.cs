namespace eShopX.Endpoints.Auth;

public sealed class AuthGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/auth";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Auth");
    }
}
