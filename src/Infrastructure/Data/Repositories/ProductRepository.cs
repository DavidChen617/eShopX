using Domain.Aggregates.Products;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class ProductRepository(EShopContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Products
            .Include(p => p.Variants).ThenInclude(v => v.Skus)
            .Include(p => p.Variants).ThenInclude(v => v.Images)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<SkuDetails?> GetSkuDetailsAsync(Guid skuId, CancellationToken cancellationToken = default)
    {
        var sku = await db.ProductSkus.FirstOrDefaultAsync(s => s.Id == skuId, cancellationToken);
        if (sku is null) return null;

        var variant = await db.ProductVariants
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.Id == sku.ProductVariantId, cancellationToken);
        if (variant is null) return null;

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == variant.ProductId, cancellationToken);
        if (product is null) return null;

        return new SkuDetails(product, variant, sku);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        Guid? categoryId,
        Audience? audience,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsQueryable();

        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (audience.HasValue) query = query.Where(p => p.Audience == audience.Value);
        if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(p => p.Variants).ThenInclude(v => v.Skus)
            .Include(p => p.Variants).ThenInclude(v => v.Images)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => await db.Products.AddAsync(product, cancellationToken);

    public void Update(Product product)
        => db.Products.Update(product);

    public void Delete(Product product)
        => db.Products.Remove(product);
}
