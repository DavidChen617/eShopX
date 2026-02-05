namespace ApplicationCore.UseCases.Orders.CreatePaidOrderFromCart;

public record CreatePaidOrderFromCartCommand(
    Guid UserId,
    string PaymentMethod) : IRequest<CreatePaidOrderFromCartResponse>;