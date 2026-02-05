namespace eShopX.Endpoints.Orders;

public class OrdersGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix => "/api/orders";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Orders")
            .RequireAuthorization();
    }
}