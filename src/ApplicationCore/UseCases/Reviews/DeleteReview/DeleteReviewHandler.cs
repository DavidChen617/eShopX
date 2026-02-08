using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Reviews.DeleteReview;

public class DeleteReviewHandler(IRepository<Review> reviewRepository)
    : IRequestHandler<DeleteReviewCommand>
{
    public async Task Handle(DeleteReviewCommand command, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review", command.ReviewId);

        if (review.UserId != command.UserId)
            throw new ForbiddenException("You can only delete your own review");

        reviewRepository.Remove(review);
        await reviewRepository.SaveChangesAsync(cancellationToken);
    }
}
