using Asp.Versioning;

namespace eShopX.Endpoints.Users;

public sealed class UsersGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/users";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Users")
            .RequireAuthorization()
            .WithApiVersionSet(apiVersionSet);
    }
}
