using Domain.Aggregates.Tags;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class TagRepository(EShopContext db) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
        => await db.Tags.OrderBy(t => t.Name).ToListAsync(ct);

    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Tag tag, CancellationToken ct = default)
        => await db.Tags.AddAsync(tag, ct);

    public void Update(Tag tag)
        => db.Tags.Update(tag);

    public void Delete(Tag tag)
        => db.Tags.Remove(tag);
}
