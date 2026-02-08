namespace ApplicationCore.UseCases.Reviews.UpdateReview;

public record UpdateReviewResponse(
    Guid ReviewId,
    int Rating,
    DateTime UpdatedAt);
