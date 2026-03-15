namespace eShopX.Endpoints.ECPay;

public sealed class EcPayGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/ecpay";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("ECPay");
    }
}
