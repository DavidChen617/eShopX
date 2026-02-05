namespace ApplicationCore.UseCases.Carts.GetCart;

public class GetCartValidator : AbstractValidator<GetCartQuery>
{
    public GetCartValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
    }
}