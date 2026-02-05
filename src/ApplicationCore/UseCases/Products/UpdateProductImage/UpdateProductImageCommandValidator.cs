namespace ApplicationCore.UseCases.Products.UpdateProductImage;

public class UpdateProductImageCommandValidator : AbstractValidator<UpdateProductImageCommand>
{
    public UpdateProductImageCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.ImageId)
            .NotEmpty().WithMessage("ImageId is required");

        RuleFor(x => x.SortOrder)
            .GreaterThan(-1)
            .When(x => x.SortOrder.HasValue)
            .WithMessage("SortOrder must be >= 0");

        RuleFor(x => x.IsPrimary)
            .NotNull()
            .When(x => !x.SortOrder.HasValue)
            .WithMessage("IsPrimary or SortOrder must be provided");

        RuleFor(x => x.SortOrder)
            .NotNull()
            .When(x => !x.IsPrimary.HasValue)
            .WithMessage("IsPrimary or SortOrder must be provided");
    }
}