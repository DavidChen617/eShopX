using Asp.Versioning;

namespace eShopX.Endpoints.Admin;

public sealed class AdminOrdersGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/admin/orders";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Admin")
            .RequireAuthorization("Admin")
            .WithApiVersionSet(apiVersionSet);
    }
}
