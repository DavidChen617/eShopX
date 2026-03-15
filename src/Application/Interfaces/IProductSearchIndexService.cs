namespace eShopX.Application.Interfaces;

public interface IProductSearchIndexService
{
    Task<ReindexProductsResult> ReindexAsync(int batchSize = 500, CancellationToken cancellationToken = default);
}

public record ReindexProductsResult(int TotalCount, int Indexed, int Failed);
