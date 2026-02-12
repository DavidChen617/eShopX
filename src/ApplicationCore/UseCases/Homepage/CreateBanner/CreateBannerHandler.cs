namespace ApplicationCore.UseCases.Homepage.CreateBanner;

public class CreateBannerHandler(
    IRepository<Banner> bannerRepository,
    IImageStorage imageStorage,
    ICacheService cacheService)
    : IRequestHandler<CreateBannerCommand, CreateBannerResponse>
{
    public async Task<CreateBannerResponse> Handle(CreateBannerCommand command, CancellationToken cancellationToken = default)
    {
        ImageUploadResult? uploadResult = null;
        var imageUrl = command.ImageUrl?.Trim() ?? string.Empty;
        if (command.Image is not null)
        {
            uploadResult = await imageStorage.UploadAsync(command.Image, cancellationToken);
            imageUrl = uploadResult.Url;
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new BadRequestException("圖片網址不能為空");
        }

        var banner = new Banner
        {
            Title = command.Title,
            ImageUrl = imageUrl,
            ImagePublicId = uploadResult?.PublicId,
            ImageFormat = uploadResult?.Format,
            ImageWidth = uploadResult?.Width,
            ImageHeight = uploadResult?.Height,
            ImageBytes = uploadResult?.Bytes,
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
