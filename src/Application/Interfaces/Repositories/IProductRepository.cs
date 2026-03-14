using eShopX.Domain.Aggregates.Products;

namespace eShopX.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        Guid? categoryId,
        Audience? audience,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Delete(Product product);
}
