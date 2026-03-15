using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Sizes;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class SizeRepository(EShopContext db) : ISizeRepository
{
    public Task<Size?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Sizes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
