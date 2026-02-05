namespace ApplicationCore.UseCases.Orders.UpdateOrderStatus;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status");
    }
}