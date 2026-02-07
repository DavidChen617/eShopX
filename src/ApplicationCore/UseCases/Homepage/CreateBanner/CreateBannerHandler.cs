using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.CreateBanner;

public class CreateBannerHandler(
    IRepository<Banner> bannerRepository,
    ICacheService cacheService)
    : IRequestHandler<CreateBannerCommand, CreateBannerResponse>
{
    public async Task<CreateBannerResponse> Handle(CreateBannerCommand command, CancellationToken cancellationToken = default)
    {
        var banner = new Banner
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            ImageUrl = command.ImageUrl,
            Link = command.Link,
            SortOrder = command.SortOrder,
            StartsAt = command.StartsAt,
            EndsAt = command.EndsAt,
            IsActive = true,
            UpdatedAt = DateTime.UtcNow
        };

        await bannerRepository.AddAsync(banner, cancellationToken);
        await bannerRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:banners", cancellationToken);

        return new CreateBannerResponse(banner.Id);
    }
}
