using Asp.Versioning;

namespace eShopX.Endpoints.Tags;

public sealed class TagsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/tags";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Tags")
            .WithApiVersionSet(apiVersionSet);
    }
}
