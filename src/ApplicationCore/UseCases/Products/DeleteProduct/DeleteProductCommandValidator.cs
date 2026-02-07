namespace ApplicationCore.UseCases.Products.DeleteProduct;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEqual(Guid.Empty).WithMessage("Seller ID is required");

        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty).WithMessage("Product ID is required");
    }
}
