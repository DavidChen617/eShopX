using CoreMesh.Result;

namespace eShopX.Endpoints.Admin.Products;

public sealed class ReindexProductsEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/reindex", Handle)
            .Produces<ApiResponse<ReindexProductsResult>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        IProductSearchIndexService indexService,
        CancellationToken ct)
    {
        var result = await indexService.ReindexAsync(cancellationToken: ct);
        return Result<ReindexProductsResult>.Ok(result).ToHttpResult();
    }
}
