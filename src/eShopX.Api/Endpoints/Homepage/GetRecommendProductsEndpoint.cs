using ApplicationCore.UseCases.Homepage.GetRecommendProducts;

namespace eShopX.Endpoints.Homepage;

public class GetRecommendProductsEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/recommend", Handle)
            .Produces<GetRecommendProductsResponse>()
            .WithDescription("取得推薦商品列表");
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? type,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ISender sender)
    {
        var result = await sender.Send(new GetRecommendProductsQuery(type ?? "homepage", page, pageSize));
        return Results.Ok(ApiResponse<GetRecommendProductsResponse>.Success(result));
    }
}