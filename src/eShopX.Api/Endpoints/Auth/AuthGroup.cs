using Asp.Versioning;

namespace eShopX.Endpoints.Auth;

public sealed class AuthGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/auth";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Auth")
            .WithApiVersionSet(apiVersionSet);
    }
}
