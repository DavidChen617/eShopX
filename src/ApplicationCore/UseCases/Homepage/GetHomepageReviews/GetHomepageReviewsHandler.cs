using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.GetHomepageReviews;

public class GetHomepageReviewsHandler(
    IReadRepository<Review> reviewRepository,
    IReadRepository<ProductImage> productImageRepository,
    IMapper mapper,
    ICacheService cacheService)
    : IRequestHandler<GetHomepageReviewsQuery, List<HomepageReviewItem>>
{
    public async Task<List<HomepageReviewItem>> Handle(
        GetHomepageReviewsQuery query,
        CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetOrSetAsync(
            "homepage:reviews",
            async ct =>
            {
                var reviews = await reviewRepository.QueryAsync(q => q
                    .Where(r => r.Rating >= 4 && r.Content != null)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(query.Limit),
                    ct);

                var dtos = mapper.Map<List<HomepageReviewDto>>(reviews);

                var productIds = dtos.Select(r => r.ProductId).Distinct().ToList();
                var productImages = await productImageRepository.ListAsync(
                    i => productIds.Contains(i.ProductId) && i.IsPrimary, ct);
                var imageMap = productImages.ToDictionary(i => i.ProductId, i => i.Url);

                var items = dtos.Select(r => new HomepageReviewItem(
                    r.ReviewId,
                    r.UserName,
                    r.UserAvatar,
                    r.ProductId,
                    r.ProductName,
                    imageMap.GetValueOrDefault(r.ProductId),
                    r.Rating,
                    r.Content,
                    r.ImageUrls,
                    r.CreatedAt)).ToList();

                return items;
            },
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return cached ?? [];
    }
}
