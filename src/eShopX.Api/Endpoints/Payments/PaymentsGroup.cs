using CoreMesh.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace eShopX.Api.Endpoints.Payments;

public sealed class PaymentsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/payments";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Payments");
    }
}
