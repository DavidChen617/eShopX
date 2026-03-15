using Asp.Versioning;

namespace eShopX.Endpoints.Admin.Products;

public sealed class AdminProductsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/admin/products";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Admin - Products")
            .RequireAuthorization("Admin")
            .WithApiVersionSet(apiVersionSet);
    }
}
