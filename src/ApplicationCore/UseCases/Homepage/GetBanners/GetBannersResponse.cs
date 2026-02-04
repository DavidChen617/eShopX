namespace ApplicationCore.UseCases.Homepage.GetBanners;

public record GetBannersResponse(List<BannerItem> Items);

public record BannerItem(
    Guid Id,
    string Title,
    string ImageUrl,
    string Link);
