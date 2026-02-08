namespace ApplicationCore.UseCases.Reviews.DeleteReview;

public record DeleteReviewCommand(Guid ReviewId, Guid UserId) : IRequest;
