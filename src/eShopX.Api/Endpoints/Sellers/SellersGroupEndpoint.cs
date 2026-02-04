namespace eShopX.Endpoints.Sellers;

public class SellersGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/api/sellers";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Sellers");
        group.RequireAuthorization();
    }
}
