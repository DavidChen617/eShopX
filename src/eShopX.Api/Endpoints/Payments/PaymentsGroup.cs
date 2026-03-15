using Asp.Versioning;

namespace eShopX.Endpoints.Payments;

public sealed class PaymentsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/v{version:apiVersion}/payments";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        group
            .WithTags("Payments")
            .WithApiVersionSet(apiVersionSet);
    }
}
