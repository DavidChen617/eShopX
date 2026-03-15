using Asp.Versioning;

namespace eShopX.Endpoints.Products;

public sealed class ProductsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/products";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Products")
            .WithApiVersionSet(apiVersionSet);
    }
}
