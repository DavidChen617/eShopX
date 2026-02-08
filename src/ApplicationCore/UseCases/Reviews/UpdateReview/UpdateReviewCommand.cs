namespace ApplicationCore.UseCases.Reviews.UpdateReview;

public record UpdateReviewCommand(
    Guid ReviewId,
    Guid UserId,
    int Rating,
    string? Content,
    bool IsAnonymous,
    List<string>? ImageUrls) : IRequest<UpdateReviewResponse>;
