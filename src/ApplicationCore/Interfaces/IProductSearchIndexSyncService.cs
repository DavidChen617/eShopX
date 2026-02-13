using eShopX.Common.Proxy;

namespace ApplicationCore.Interfaces;

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface IProductSearchIndexSyncService: IInterceptable
{
    Task UpsertProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
