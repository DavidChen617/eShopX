namespace ApplicationCore.UseCases.Products.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEqual(Guid.Empty).WithMessage("Seller ID is required");

        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty).WithMessage("Product ID is required");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).When(x => x.CategoryId.HasValue)
            .WithMessage("Category ID is invalid");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaxLength(200).WithMessage("Product name must be at most 200 characters");

        RuleFor(x => x.Description)
            .MaxLength(2000).WithMessage("Product description must be at most 2000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .Must(price => price >= 0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThan(-1).WithMessage("Stock quantity must be zero or greater");
    }
}