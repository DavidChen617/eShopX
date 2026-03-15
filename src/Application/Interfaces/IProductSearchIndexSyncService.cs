namespace eShopX.Application.Interfaces;

public interface IProductSearchIndexSyncService
{
    Task UpsertProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
