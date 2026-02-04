namespace ApplicationCore.UseCases.Orders.CreatePaidOrderFromCart;

public class CreatePaidOrderFromCartValidator : AbstractValidator<CreatePaidOrderFromCartCommand>
{
    public CreatePaidOrderFromCartValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("PaymentMethod is required");
    }
}
