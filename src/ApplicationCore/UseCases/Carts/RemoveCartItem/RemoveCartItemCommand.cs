namespace ApplicationCore.UseCases.Carts.RemoveCartItem;

public record RemoveCartItemCommand(
    Guid UserId,
    Guid ProductId
) : IRequest;
