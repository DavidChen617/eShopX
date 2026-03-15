namespace eShopX.Endpoints.Products;

public sealed class ProductsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/products";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Products");
    }
}
