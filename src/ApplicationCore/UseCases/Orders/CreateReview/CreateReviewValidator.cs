namespace ApplicationCore.UseCases.Reviews.CreateReview;

public class CreateReviewValidator:  AbstractValidator<CreateReviewCommand>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");
        
        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("OrderItemId is required");
        
        RuleFor(x => x.Rating)                                                                                                                         
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
    }
}
