using ApplicationCore.UseCases.Products.GetProductImages;

namespace eShopX.Endpoints.Products;

public class GetProductImagesEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("{productId}/images", Handle)
            .Produces<GetProductImagesResponse>()
            .AllowAnonymous()
            .WithDescription("取得商品圖片列表");
    }

    private static async Task<IResult> Handle([FromRoute] Guid productId, ISender sender)
    {
        var result = await sender.Send(new GetProductImagesQuery(productId));
        return Results.Ok(ApiResponse<GetProductImagesResponse>.Success(result));
    }
}