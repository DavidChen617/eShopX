namespace ApplicationCore.UseCases.Products.GetProductReview;

public record GetProductReviewsQuery(Guid ProductId, int Page = 1, int PageSize = 10) :
    IRequest<GetProductReviewsResponse>;
