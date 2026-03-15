using eShopX.Application.Interfaces.Repositories;

namespace Infrastructure.Search.Elasticsearch;

public class ProductReindexer(
    IProductRepository productRepository,
    IProductSearchIndexSynchronizer synchronizer) : IProductSearchIndexService
{
    public async Task<ReindexProductsResult> ReindexAsync(int batchSize = 500, CancellationToken cancellationToken = default)
    {
        var page = 1;
        var indexed = 0;
        var failed = 0;
        int totalCount;

        do
        {
            var (items, total) = await productRepository.GetPagedAsync(
                categoryId: null,
                audience: null,
                isActive: null,
                page: page,
                pageSize: batchSize,
                cancellationToken: cancellationToken);

            totalCount = total;

            foreach (var product in items)
            {
                try
                {
                    await synchronizer.UpsertProductAsync(product.Id, cancellationToken);
                    indexed++;
                }
                catch
                {
                    failed++;
                }
            }

            page++;
        } while ((page - 1) * batchSize < totalCount);

        return new ReindexProductsResult(totalCount, indexed, failed);
    }
}
