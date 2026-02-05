using ApplicationCore.UseCases.Products.GetProductDetail;

namespace eShopX.Endpoints.Products;

public class GetProductDetailEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("{id}", Handle)
            .Produces<GetProductDetailResponse>()
            .WithDescription("取得單一商品詳細資訊")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new GetProductDetailQuery(id));
        return Results.Ok(ApiResponse<GetProductDetailResponse>.Success(result));
    }
}