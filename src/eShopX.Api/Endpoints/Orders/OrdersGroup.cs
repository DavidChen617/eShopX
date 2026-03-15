using CoreMesh.Endpoints;

namespace eShopX.Api.Endpoints.Orders;

public sealed class OrdersGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/orders";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Orders").RequireAuthorization();
    }
}
