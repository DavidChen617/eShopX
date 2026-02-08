namespace ApplicationCore.UseCases.Reviews.CreateReview;

public class CreateReviewProfile: Profile
{
    public CreateReviewProfile()
    {
        CreateMap<Review, CreateReviewResponse>()
            .ConstructUsing(src => new CreateReviewResponse(
            src.Id, src.ProductId, src.Rating, src.CreatedAt));
    }
}
