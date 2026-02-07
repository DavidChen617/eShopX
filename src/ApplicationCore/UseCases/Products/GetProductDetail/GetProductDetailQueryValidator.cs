namespace ApplicationCore.UseCases.Products.GetProductDetail;

public class GetProductDetailQueryValidator : AbstractValidator<GetProductDetailQuery>
{
    public GetProductDetailQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty).WithMessage("Product ID is required");
    }
}
