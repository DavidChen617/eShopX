using Asp.Versioning;

namespace eShopX.Endpoints.Orders;

public sealed class OrdersGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/orders";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Orders")
            .RequireAuthorization()
            .WithApiVersionSet(apiVersionSet);
    }
}
