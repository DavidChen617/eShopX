namespace ApplicationCore.UseCases.Reviews.UpdateReview;

public class UpdateReviewValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("ReviewId is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
    }
}
