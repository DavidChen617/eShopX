using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.GetRecommendProducts;

public class GetRecommendProductsHandler(
    IReadRepository<ProductRecommend> recommendRepository,
    IReadRepository<Product> productRepository,
    IReadRepository<ProductImage> productImageRepository,
    ICacheService cacheService)
    : IRequestHandler<GetRecommendProductsQuery, GetRecommendProductsResponse>
{
    public async Task<GetRecommendProductsResponse> Handle(
        GetRecommendProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page > 0 ? query.Page.Value : 1;
        var size = query.PageSize > 0 ? query.PageSize.Value : 10;
        size = Math.Min(size, 50);
        var cacheKey = $"homepage:recommend:{query.RecommendType}:{page}:{size}";

        return (await cacheService.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                var now = DateTime.UtcNow;
                var totalCount = await recommendRepository.CountAsync(r =>
                        r.IsActive &&
                        r.RecommendType == query.RecommendType &&
                        (r.StartsAt == null || r.StartsAt <= now) &&
                        (r.EndsAt == null || r.EndsAt >= now),
                    ct);

                if (totalCount == 0)
                    return new GetRecommendProductsResponse(page, size, 0, 0, []);

                var recommends = await recommendRepository.QueryAsync(q =>
                        q.Where(r => r.IsActive)
                            .Where(r => r.RecommendType == query.RecommendType)
                            .Where(r => r.StartsAt == null || r.StartsAt <= now)
                            .Where(r => r.EndsAt == null || r.EndsAt >= now)
                            .OrderBy(r => r.SortOrder)
                            .Skip((page - 1) * size)
                            .Take(size)
                            .Select(r => r.ProductId),
                    ct);

                var productIds = recommends.ToList();

                var products = await productRepository.QueryAsync(q =>
                        q.Where(p => productIds.Contains(p.Id) && p.IsActive)
                            .Select(p => new { p.Id, p.Name, p.Price, p.StockQuantity }),
                    ct);

                var productMap = products.ToDictionary(p => p.Id);

                var primaryImages = await productImageRepository.ListAsync(
                    x => productIds.Contains(x.ProductId) && x.IsPrimary,
                    ct);

                var imageMap = primaryImages
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.First().Url);

                var items = productIds
                    .Where(id => productMap.ContainsKey(id))
                    .Select(id =>
                    {
                        var product = productMap[id];
                        imageMap.TryGetValue(id, out var imageUrl);
                        return new RecommendProductItem(
                            product.Id,
                            product.Name,
                            imageUrl,
                            product.Price,
                            null,
                            product.StockQuantity);
                    }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / size);

                return new GetRecommendProductsResponse(page, size, totalCount, totalPages, items);
            },
            TimeSpan.FromMinutes(2),
            cancellationToken))!;
    }
}
