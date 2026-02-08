using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Reviews.UpdateReview;

public class UpdateReviewHandler(
    IRepository<Review> reviewRepository,
    IRepository<ReviewImage> reviewImageRepository)
    : IRequestHandler<UpdateReviewCommand, UpdateReviewResponse>
{
    public async Task<UpdateReviewResponse> Handle(
        UpdateReviewCommand command,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review", command.ReviewId);

        if (review.UserId != command.UserId)
            throw new ForbiddenException("You can only update your own review");

        review.Rating = command.Rating;
        review.Content = command.Content;
        review.IsAnonymous = command.IsAnonymous;
        review.UpdatedAt = DateTime.UtcNow;

        // Update images
        var existingImages = await reviewImageRepository.ListAsync(
            i => i.ReviewId == command.ReviewId, cancellationToken);

        reviewImageRepository.RemoveRange(existingImages);

        if (command.ImageUrls?.Count > 0)
        {
            var newImages = command.ImageUrls
                .Select((url, index) => new ReviewImage
                {
                    ReviewId = review.Id,
                    Url = url,
                    SortOrder = index
                })
                .ToList();

            await reviewImageRepository.AddRangeAsync(newImages, cancellationToken);
        }

        reviewRepository.Update(review);
        await reviewRepository.SaveChangesAsync(cancellationToken);

        return new UpdateReviewResponse(review.Id, review.Rating, review.UpdatedAt!.Value);
    }
}
