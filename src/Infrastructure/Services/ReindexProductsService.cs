using eShopX.Application.Interfaces;

namespace Infrastructure.Services;

public class ReindexProductsService : IProductSearchIndexService
{
    // TODO: Reimplement using IProductRepository and IElasticsearchClient
    public Task<ReindexProductsResult> ReindexAsync(int batchSize = 500, CancellationToken cancellationToken = default)
        => Task.FromResult(new ReindexProductsResult(0, 0, 0));
}
