namespace ApplicationCore.UseCases.Carts.ClearCart;

public record ClearCartCommand(
    Guid UserId
) : IRequest;
