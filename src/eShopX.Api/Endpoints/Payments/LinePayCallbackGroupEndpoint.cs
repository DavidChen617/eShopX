namespace eShopX.Endpoints.Payments;

public class LinePayCallbackGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/api/payments/line/callback";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("LinePay");
    }
}
