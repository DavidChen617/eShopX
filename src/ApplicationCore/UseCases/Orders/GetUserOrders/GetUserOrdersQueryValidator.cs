namespace ApplicationCore.UseCases.Orders.GetUserOrders;

public class GetUserOrdersQueryValidator : AbstractValidator<GetUserOrdersQuery>
{
    public GetUserOrdersQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("User ID is required");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0");
    }
}