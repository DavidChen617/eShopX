namespace eShopX.Endpoints.Users;

public class UsersGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/api/users";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Users");
        group.RequireAuthorization();
    }
}