using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.UpdateBanner;

public class UpdateBannerHandler(
    IRepository<Banner> bannerRepository,
    ICacheService cacheService)
    : IRequestHandler<UpdateBannerCommand, UpdateBannerResponse>
{
    public async Task<UpdateBannerResponse> Handle(UpdateBannerCommand command, CancellationToken cancellationToken = default)
    {
        var banner = await bannerRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Banner", command.Id);

        banner.Title = command.Title;
        banner.ImageUrl = command.ImageUrl;
        banner.Link = command.Link;
        banner.SortOrder = command.SortOrder;
        banner.IsActive = command.IsActive;
        banner.StartsAt = command.StartsAt;
        banner.EndsAt = command.EndsAt;
        banner.UpdatedAt = DateTime.UtcNow;

        bannerRepository.Update(banner);
        await bannerRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:banners", cancellationToken);

        return new UpdateBannerResponse(true);
    }
}