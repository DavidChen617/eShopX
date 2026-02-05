namespace ApplicationCore.UseCases.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId is required");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId is invalid");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaxLength(200).WithMessage("Product name must be at most 200 characters");

        RuleFor(x => x.Description)
            .MaxLength(2000).WithMessage("Product description must be at most 2000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThan(-1).WithMessage("Stock quantity must be zero or greater");
    }
}