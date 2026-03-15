namespace eShopX.Endpoints.Cart;

public sealed class CartGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/cart";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Cart").RequireAuthorization();
    }
}
