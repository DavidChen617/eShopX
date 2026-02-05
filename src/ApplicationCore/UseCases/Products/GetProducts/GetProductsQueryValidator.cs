namespace ApplicationCore.UseCases.Products.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .Must(page => page is null || page > 0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(size => size is null || size > 0)
            .WithMessage("Page size must be greater than 0");

        RuleFor(x => x.MinPrice)
            .Must(min => min is null || min >= 0)
            .WithMessage("Minimum price must be greater than or equal to 0");

        RuleFor(x => x.MaxPrice)
            .Must(max => max is null || max >= 0)
            .WithMessage("Maximum price must be greater than or equal to 0");

        RuleFor(x => x.MaxPrice)
            .AddRule((query, max) =>
                query.MinPrice is null || max is null || max >= query.MinPrice,
                "Maximum price must be greater than or equal to minimum price");
    }
}