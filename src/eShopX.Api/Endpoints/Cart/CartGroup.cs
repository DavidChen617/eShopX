using Asp.Versioning;

namespace eShopX.Endpoints.Cart;

public sealed class CartGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/cart";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Cart")
            .RequireAuthorization()
            .WithApiVersionSet(apiVersionSet);
    }
}
