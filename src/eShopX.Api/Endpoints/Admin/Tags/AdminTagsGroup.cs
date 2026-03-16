using Asp.Versioning;

namespace eShopX.Endpoints.Admin.Tags;

public sealed class AdminTagsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/admin/tags";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Admin - Tags")
            .RequireAuthorization("Admin")
            .WithApiVersionSet(apiVersionSet);
    }
}
