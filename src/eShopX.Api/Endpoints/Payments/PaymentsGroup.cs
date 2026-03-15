namespace eShopX.Endpoints.Payments;

public sealed class PaymentsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/payments";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Payments");
    }
}
