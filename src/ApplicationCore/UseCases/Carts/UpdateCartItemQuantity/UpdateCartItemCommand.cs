namespace ApplicationCore.UseCases.Carts.UpdateCartItemQuantity;

public record UpdateCartItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity
) : IRequest<UpdateCartItemResponse>;
