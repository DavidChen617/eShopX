namespace ApplicationCore.UseCases.Homepage.UpdateBanner;

public class UpdateBannerHandler(
    IRepository<Banner> bannerRepository,
    IImageStorage imageStorage,
    ICacheService cacheService)
    : IRequestHandler<UpdateBannerCommand, UpdateBannerResponse>
{
    public async Task<UpdateBannerResponse> Handle(UpdateBannerCommand command, CancellationToken cancellationToken = default)
    {
        var banner = await bannerRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Banner", command.Id);

        var oldImageUrl = banner.ImageUrl;
        var oldPublicId = banner.ImagePublicId;

        banner.Title = command.Title;
        banner.Link = command.Link;
        banner.SortOrder = command.SortOrder;
        banner.IsActive = command.IsActive;
        banner.StartsAt = command.StartsAt;
        banner.EndsAt = command.EndsAt;
        banner.UpdatedAt = DateTime.UtcNow;

        if (command.Image is not null)
        {
            var uploadResult = await imageStorage.UploadAsync(command.Image, cancellationToken);
            banner.ImageUrl = uploadResult.Url;
            banner.ImagePublicId = uploadResult.PublicId;
            banner.ImageFormat = uploadResult.Format;
            banner.ImageWidth = uploadResult.Width;
            banner.ImageHeight = uploadResult.Height;
            banner.ImageBytes = uploadResult.Bytes;
        }
        else if (!string.IsNullOrWhiteSpace(command.ImageUrl))
        {
            banner.ImageUrl = command.ImageUrl.Trim();
            if (!string.Equals(banner.ImageUrl, oldImageUrl, StringComparison.Ordinal))
            {
                banner.ImagePublicId = null;
                banner.ImageFormat = null;
                banner.ImageWidth = null;
                banner.ImageHeight = null;
                banner.ImageBytes = null;
            }
        }

        bannerRepository.Update(banner);
        await bannerRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:banners", cancellationToken);

        var newPublicId = banner.ImagePublicId;
        if (!string.IsNullOrWhiteSpace(oldPublicId) &&
            !string.Equals(oldPublicId, newPublicId, StringComparison.Ordinal))
        {
            await imageStorage.DeleteAsync(oldPublicId, cancellationToken);
        }

        return new UpdateBannerResponse(true);
    }
}
