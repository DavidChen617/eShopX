namespace ApplicationCore.UseCases.Products.UploadProductImage;

public class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("Image is required");

        RuleFor(x => x.Image.FileName)
            .NotEmpty().WithMessage("FileName is required");

        RuleFor(x => x.Image.ContentType)
            .NotEmpty().WithMessage("ContentType is required");

        RuleFor(x => x.Image.Length)
            .GreaterThan(0).WithMessage("Image length must be greater than 0");

        RuleFor(x => x.Image.Content)
            .NotEmpty().WithMessage("Image content is required");

        RuleFor(x => x.SortOrder)
            .GreaterThan(-1).WithMessage("SortOrder must be >= 0");
    }
}