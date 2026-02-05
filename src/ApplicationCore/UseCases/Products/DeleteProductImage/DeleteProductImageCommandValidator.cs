namespace ApplicationCore.UseCases.Products.DeleteProductImage;

public class DeleteProductImageCommandValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.ImageId)
            .NotEmpty().WithMessage("ImageId is required");
    }
}