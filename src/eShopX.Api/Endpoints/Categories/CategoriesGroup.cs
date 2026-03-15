using Asp.Versioning;

namespace eShopX.Endpoints.Categories;

public sealed class CategoriesGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/categories";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Categories")
            .WithApiVersionSet(apiVersionSet);
    }
}
