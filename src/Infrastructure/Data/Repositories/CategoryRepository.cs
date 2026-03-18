using Domain.Aggregates.Categories;

namespace Infrastructure.Data.Repositories;

public class CategoryRepository(EShopContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Categories.OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        => await db.Categories.AddAsync(category, cancellationToken);

    public void Update(Category category)
        => db.Categories.Update(category);

    public void Delete(Category category)
        => db.Categories.Remove(category);
}
