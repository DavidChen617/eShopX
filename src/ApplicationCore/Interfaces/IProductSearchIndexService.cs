using eShopX.Common.Proxy;

namespace ApplicationCore.Interfaces;
[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface IProductSearchIndexService: IInterceptable
{
    Task<ReindexProductsResult> ReindexAsync(int batchSize = 500, CancellationToken cancellationToken = default);
}

public record ReindexProductsResult(int TotalCount, int IndexedCount, int FailedCount);
