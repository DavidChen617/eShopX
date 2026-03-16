using eShopX.Domain.Aggregates.Sizes;

namespace eShopX.Application.Interfaces.Repositories;

public interface ISizeRepository
{
    Task<IReadOnlyList<Size>> GetAllAsync(SizeType? filter = null, CancellationToken ct = default);
    Task<Size?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Size size, CancellationToken ct = default);
    void Update(Size size);
    void Delete(Size size);
}
