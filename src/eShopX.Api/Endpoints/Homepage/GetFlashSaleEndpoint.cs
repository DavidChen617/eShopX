using ApplicationCore.UseCases.Homepage.GetFlashSale;

namespace eShopX.Endpoints.Homepage;

public class GetFlashSaleEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/flash-sale", Handle)
            .Produces<GetFlashSaleResponse>()
            .WithDescription("取得進行中的秒殺活動");
    }

    private static async Task<IResult> Handle(ISender sender)
    {
        var result = await sender.Send(new GetFlashSaleQuery());
        if (result == null)
            return Results.Ok(ApiResponse<GetFlashSaleResponse?>.Success(null));
        return Results.Ok(ApiResponse<GetFlashSaleResponse>.Success(result));
    }
}