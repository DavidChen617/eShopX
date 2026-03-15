using CoreMesh.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace eShopX.Api.Endpoints.Admin;

public sealed class AdminOrdersGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/admin/orders";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Admin").RequireAuthorization("Admin");
    }
}
