namespace eShopX.Endpoints.Payments;

public class PayPalCallbackGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/pay/paypal";
    public void Configure(RouteGroupBuilder group) => group.WithTags("PayPal");
}
