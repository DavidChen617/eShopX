namespace eShopX.Endpoints.Products;

public class ProductsGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix => "/api/products";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Products")
            .RequireAuthorization();
    }
}