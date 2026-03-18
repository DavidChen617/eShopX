using Domain.Aggregates.Categories;

namespace Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Update(Category category);
    void Delete(Category category);
}
