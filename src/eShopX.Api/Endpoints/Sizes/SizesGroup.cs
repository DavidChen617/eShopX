using Asp.Versioning;

namespace eShopX.Endpoints.Sizes;

public sealed class SizesGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/sizes";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Sizes")
            .WithApiVersionSet(apiVersionSet);
    }
}
