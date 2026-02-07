namespace eShopX.Endpoints.FlashSales;

public class FlashSaleGroupEndpoint: IGroupEndpoint
{
    public string GroupPrefix => "/api/flash-sale";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("FlashSale")
            .RequireAuthorization()
            .RequireRateLimiting("FlashSale");
    }
}
