namespace eShopX.Endpoints.Payments;

public class LinePayCallbackGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "/pay/line";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("LinePay");
    }
}