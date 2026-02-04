namespace ApplicationCore.UseCases.Homepage.CreateBanner;

public record CreateBannerCommand(
    string Title,
    string ImageUrl,
    string Link,
    int SortOrder,
    DateTime? StartsAt,
    DateTime? EndsAt) : IRequest<CreateBannerResponse>;
