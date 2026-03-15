using Asp.Versioning;

namespace eShopX.Endpoints.Admin.Categories;

public sealed class AdminCategoriesGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/admin/categories";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Admin - Categories")
            .RequireAuthorization("Admin")
            .WithApiVersionSet(apiVersionSet);
    }
}
