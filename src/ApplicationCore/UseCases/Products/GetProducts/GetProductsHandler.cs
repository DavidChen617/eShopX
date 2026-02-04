using System.Linq.Expressions;

using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Products.GetProducts;

public class GetProductsHandler(
    IRepository<Product> productRepository,
    IRepository<ProductImage> productImageRepository,
    ICacheService cacheService) : IRequestHandler<GetProductsQuery, GetProductResponse>
{
    public async Task<GetProductResponse> Handle(GetProductsQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page > 0 ? query.Page.Value : 1;
        var size = query.PageSize > 0 ? query.PageSize.Value : 10;
        size = Math.Min(size, 50);
        var cacheKey = string.Join(":",
            "products:list",
            query.Keyword ?? "",
            query.IsActive?.ToString() ?? "",
            query.MinPrice?.ToString() ?? "",
            query.MaxPrice?.ToString() ?? "",
            page,
            size,
            query.SellerId?.ToString() ?? "",
            query.CategoryId?.ToString() ?? "");

        return await cacheService.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                Expression<Func<Product, bool>> filter = product =>
                    (string.IsNullOrEmpty(query.Keyword) || product.Name.Contains(query.Keyword)) &&
                    (query.IsActive == null || product.IsActive == query.IsActive) &&
                    (query.MinPrice == null || product.Price >= query.MinPrice) &&
                    (query.MaxPrice == null || product.Price <= query.MaxPrice) &&
                    (query.SellerId == null || product.SellerId == query.SellerId) &&
                    (query.CategoryId == null || product.CategoryId == query.CategoryId);

                var totalCount = await productRepository.CountAsync(filter, ct);

                if (totalCount == 0)
                {
                    return new GetProductResponse(page, size, 0, 0, []);
                }

                var products = await productRepository.QueryAsync(q =>
                        q.Where(filter)
                            .OrderByDescending(p => p.CreatedAt)
                            .Skip((page - 1) * size)
                            .Take(size)
                            .Select(p => new
                            {
                                p.Id,
                                p.CategoryId,
                                p.Name,
                                p.Description,
                                p.Price,
                                p.StockQuantity,
                                p.IsActive
                            }), ct);

                var productIds = products.Select(p => p.Id).ToList();
                var primaryImages = await productImageRepository.ListAsync(
                    x => productIds.Contains(x.ProductId) && x.IsPrimary,
                    ct);

                var primaryImageMap = primaryImages
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.CreatedAt)
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.First().Url);

                var items = products.Select(p =>
                {
                    primaryImageMap.TryGetValue(p.Id, out var imageUrl);
                    return new GetProductItems(
                        p.Id,
                        p.CategoryId,
                        p.Name,
                        p.Description ?? string.Empty,
                        p.Price,
                        p.StockQuantity,
                        p.IsActive,
                        imageUrl);
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / size);

                return new GetProductResponse(page, size, totalCount, totalPages, items);
            },
            TimeSpan.FromMinutes(2),
            cancellationToken);
    }
}
