namespace ApplicationCore.UseCases.Homepage.GetHomepageReviews;

public record GetHomepageReviewsQuery(int Limit = 10) : IRequest<List<HomepageReviewItem>>;
