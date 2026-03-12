namespace ApplicationCore.UseCases.Products.GetProductReview;

public class GetProductReviewProfile: Profile
{
    public GetProductReviewProfile()
    {
        CreateMap<Review, ReviewItem>().ConstructUsing(src =>
            new ReviewItem(
            src.Id,
            src.IsAnonymous ? null : src.User?.Name,
            src.Rating, src.Content,
            (src.Images ?? []).OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            src.CreatedAt));
    }
}
