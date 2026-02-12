using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.UpdateBanner;

public record UpdateBannerCommand(
    Guid Id,
    string Title,
    string? ImageUrl,
    ImageUploadRequest? Image,
    string Link,
    int SortOrder,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? EndsAt) : IRequest<UpdateBannerResponse>;
