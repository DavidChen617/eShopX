using eShopX.Domain.Aggregates.Tags;

namespace eShopX.Application.Interfaces.Repositories;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Tag tag, CancellationToken ct = default);
    void Update(Tag tag);
    void Delete(Tag tag);
}
