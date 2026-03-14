using eShopX.Domain.Aggregates.Sizes;

namespace eShopX.Application.Interfaces.Repositories;

public interface ISizeRepository
{
    Task<Size?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
