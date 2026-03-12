namespace eShopX.Endpoints.Payments;

public class PayPalCallbackGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/api/payments/paypal/callback";
    public void Configure(RouteGroupBuilder group) => group.WithTags("PayPal");
}
