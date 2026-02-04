namespace eShopX.Endpoints.Carts;

public class CartGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "api/carts";
    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Carts")
            .RequireAuthorization();
    }
}
