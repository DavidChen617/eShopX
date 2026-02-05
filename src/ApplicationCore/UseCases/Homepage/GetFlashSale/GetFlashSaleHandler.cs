using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.GetFlashSale;

public class GetFlashSaleHandler(
    IReadRepository<FlashSale> flashSaleRepository,
    IReadRepository<FlashSaleSlot> slotRepository,
    IReadRepository<FlashSaleItem> itemRepository,
    IReadRepository<Product> productRepository,
    IReadRepository<ProductImage> productImageRepository,
    ICacheService cacheService)
    : IRequestHandler<GetFlashSaleQuery, GetFlashSaleResponse?>
{
    public async Task<GetFlashSaleResponse?> Handle(GetFlashSaleQuery query, CancellationToken cancellationToken = default)
    {
        var cached = await cacheService.GetOrSetAsync(
            "homepage:flashsale",
            async ct =>
            {
                var now = DateTime.UtcNow;

                var flashSale = await flashSaleRepository.QueryAsync(q =>
                        q.Where(f => f.IsActive)
                            .Where(f => f.StartsAt <= now && f.EndsAt >= now)
                            .OrderBy(f => f.StartsAt)
                            .Take(1),
                    ct);

                var sale = flashSale.FirstOrDefault();
                if (sale == null)
                    return new CacheBox<GetFlashSaleResponse>(null);

                var slots = await slotRepository.QueryAsync(q =>
                        q.Where(s => s.FlashSaleId == sale.Id)
                            .OrderBy(s => s.SortOrder)
                            .Select(s => new { s.Id, s.Label, s.StartsAt, s.EndsAt }),
                    ct);

                var slotItems = slots.Select(s =>
                {
                    var status = s.EndsAt < now ? "ended" :
                                 s.StartsAt <= now ? "live" : "upcoming";
                    return new FlashSaleSlotItem(s.Id, s.Label, s.StartsAt, s.EndsAt, status);
                }).ToList();

                var items = await itemRepository.QueryAsync(q =>
                        q.Where(i => i.FlashSaleId == sale.Id)
                            .OrderBy(i => i.SortOrder)
                            .Select(i => new
                            {
                                i.ProductId,
                                i.FlashPrice,
                                i.StockTotal,
                                i.StockRemaining,
                                i.Badge
                            }),
                    ct);

                var productIds = items.Select(i => i.ProductId).ToList();

                var products = await productRepository.QueryAsync(q =>
                        q.Where(p => productIds.Contains(p.Id))
                            .Select(p => new { p.Id, p.Name, p.Price }),
                    ct);

                var productMap = products.ToDictionary(p => p.Id);

                var primaryImages = await productImageRepository.ListAsync(
                    x => productIds.Contains(x.ProductId) && x.IsPrimary,
                    ct);

                var imageMap = primaryImages
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.First().Url);

                var productItems = items.Select(i =>
                {
                    productMap.TryGetValue(i.ProductId, out var product);
                    imageMap.TryGetValue(i.ProductId, out var imageUrl);

                    return new FlashSaleProductItem(
                        i.ProductId,
                        product?.Name ?? string.Empty,
                        imageUrl,
                        i.FlashPrice,
                        product?.Price ?? 0,
                        i.StockTotal,
                        i.StockRemaining,
                        i.Badge);
                }).ToList();

                return new CacheBox<GetFlashSaleResponse>(new GetFlashSaleResponse(
                    sale.Id,
                    sale.Title,
                    sale.Subtitle,
                    sale.StartsAt,
                    sale.EndsAt,
                    slotItems,
                    productItems));
            },
            TimeSpan.FromMinutes(2),
            cancellationToken);
        return cached.Value;
    }
}