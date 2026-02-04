using ApplicationCore.UseCases.Products.GetProducts;

namespace eShopX.Endpoints.Products;

public class GetProductsEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("", Handle)
            .Produces<GetProductResponse>()
            .WithDescription("查詢商品列表，支援關鍵字、價格區間、上架狀態篩選")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? keyword,
        [FromQuery] bool? isActive,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] Guid? categoryId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ISender sender)
    {
        var result = await sender.Send(new GetProductsQuery(
            keyword,
            isActive,
            minPrice,
            maxPrice,
            page,
            pageSize,
            null,
            categoryId));
        return Results.Ok(ApiResponse<GetProductResponse>.Success(result));
    }
}
