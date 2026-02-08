namespace ApplicationCore.UseCases.Products.GetProductReview;

public class GetProductReviewsHandler(
    IReadRepository<Review> reviewRepository,
    IMapper mapper):
    IRequestHandler<GetProductReviewsQuery, GetProductReviewsResponse>
{
    public async Task<GetProductReviewsResponse> Handle(GetProductReviewsQuery query, CancellationToken cancellationToken = default)
    {
        var reviews = await reviewRepository.QueryAsync(q =>
            q.Where(r => r.ProductId == query.ProductId)
                .OrderBy(r => r.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(r => mapper.Map<Review, ReviewItem>(r)),
                cancellationToken);
        
        var totalCount = await reviewRepository.CountAsync(r => r.ProductId == query.ProductId,  cancellationToken);

        var avgRating = totalCount > 0
            ? await reviewRepository.QueryAsync(q =>
                    q.Where(r => r.ProductId == query.ProductId)
                        .Select(r => (decimal)r.Rating),
                cancellationToken).ContinueWith(t => t.Result.Average(), cancellationToken)
            : 0;
        
        return new GetProductReviewsResponse(reviews,  totalCount, Math.Round(avgRating, 1));
    }
}
