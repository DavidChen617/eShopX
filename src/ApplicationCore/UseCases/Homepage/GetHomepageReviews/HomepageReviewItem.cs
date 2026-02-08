namespace ApplicationCore.UseCases.Homepage.GetHomepageReviews;

public record HomepageReviewItem(
    Guid ReviewId,
    string? UserName,
    string? UserAvatar,
    Guid ProductId,
    string ProductName,
    string? ProductImage,
    int Rating,
    string? Content,
    List<string> ImageUrls,
    DateTime CreatedAt);
