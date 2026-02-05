namespace ApplicationCore.UseCases.Products.GetProductImages;

public class GetProductImagesQueryValidator : AbstractValidator<GetProductImagesQuery>
{
    public GetProductImagesQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotNull().WithMessage("ProductId is required")
            .NotEmpty().WithMessage("ProductId is required");
    }
}