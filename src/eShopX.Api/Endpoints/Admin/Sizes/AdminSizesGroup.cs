using Asp.Versioning;

namespace eShopX.Endpoints.Admin.Sizes;

public sealed class AdminSizesGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/admin/sizes";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Admin - Sizes")
            .RequireAuthorization("Admin")
            .WithApiVersionSet(apiVersionSet);
    }
}
