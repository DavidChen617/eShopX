using Domain.Aggregates.Sizes;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class SizeRepository(EShopContext db) : ISizeRepository
{
    public async Task<IReadOnlyList<Size>> GetAllAsync(SizeType? filter = null, CancellationToken ct = default)
    {
        var query = db.Sizes.AsQueryable();
        if (filter.HasValue)
            query = query.Where(s => (s.Type & filter.Value) == filter.Value);
        return await query.OrderBy(s => s.Name).ToListAsync(ct);
    }

    public Task<Size?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Sizes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(Size size, CancellationToken ct = default)
        => await db.Sizes.AddAsync(size, ct);

    public void Update(Size size)
        => db.Sizes.Update(size);

    public void Delete(Size size)
        => db.Sizes.Remove(size);
}
