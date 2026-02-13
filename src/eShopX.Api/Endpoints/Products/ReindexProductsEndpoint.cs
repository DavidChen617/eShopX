using ApplicationCore.Interfaces;

namespace eShopX.Endpoints.Products;

public class ReindexProductsEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/reindex", Handle)
            .RequireAuthorization("Admin")
            .WithDescription("重建商品 Elasticsearch 索引");
    }

    private static async Task<IResult> Handle(
        [FromQuery] int? batchSize,
        [FromServices] IProductSearchIndexService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ReindexAsync(batchSize ?? 500, cancellationToken);
        return Results.Ok(ApiResponse<ReindexProductsResult>.Success(result));
    }
}
