namespace ApplicationCore.UseCases.Homepage.CreateBanner;

public record CreateBannerCommand(
    string Title,
    string? ImageUrl,
    ImageUploadRequest? Image,
    string Link,
    int SortOrder,
    DateTime? StartsAt,
    DateTime? EndsAt) : IRequest<CreateBannerResponse>;
