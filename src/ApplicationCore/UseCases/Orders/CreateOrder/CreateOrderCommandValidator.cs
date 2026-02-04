namespace ApplicationCore.UseCases.Orders.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("User ID is required");

        RuleFor(x => x.ShippingName)
            .NotEmpty().WithMessage("Shipping name is required");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required");

        RuleFor(x => x.ShippingPhone)
            .NotEmpty().WithMessage("Shipping phone is required");

        RuleFor(x => x.Items)
            .Must(items => items is { Count: > 0 })
            .WithMessage("Order items are required");

        RuleFor(x => x.Items)
            .Must(items => items.All(i => i.ProductId != Guid.Empty))
            .WithMessage("Order item product ID is required")
            .When(x => x.Items is not null);

        RuleFor(x => x.Items)
            .Must(items => items.All(i => i.Quantity > 0))
            .WithMessage("Order item quantity must be greater than 0")
            .When(x => x.Items is not null);
    }
}
