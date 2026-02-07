namespace eShopX.Endpoints.Payments;

public class PaymentsGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/api/payments";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Payments");
        group.RequireAuthorization();
    }
}
