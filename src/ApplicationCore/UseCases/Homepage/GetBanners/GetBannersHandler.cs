namespace ApplicationCore.UseCases.Homepage.GetBanners;

public class GetBannersHandler(
    IReadRepository<Banner> bannerRepository,
    ICacheService cacheService)
    : IRequestHandler<GetBannersQuery, GetBannersResponse>
{
    public async Task<GetBannersResponse> Handle(GetBannersQuery query, CancellationToken cancellationToken = default)
    {
        return (await cacheService.GetOrSetAsync(
            "homepage:banners",
            async ct =>
            {
                var now = DateTime.UtcNow;

                var banners = await bannerRepository.QueryAsync(q =>
                        q.Where(b => b.IsActive)
                            .Where(b => b.StartsAt == null || b.StartsAt <= now)
                            .Where(b => b.EndsAt == null || b.EndsAt >= now)
                            .OrderBy(b => b.SortOrder)
                            .Select(b => new BannerItem(b.Id, b.Title, b.ImageUrl, b.Link)),
                    ct);

                return new GetBannersResponse(banners.ToList());
            },
            TimeSpan.FromMinutes(2),
            cancellationToken))!;
    }
}
