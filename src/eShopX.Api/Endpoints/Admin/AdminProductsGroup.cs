namespace eShopX.Endpoints.Admin;

public sealed class AdminProductsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/admin/products";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Admin - Products").RequireAuthorization("Admin");
    }
}
