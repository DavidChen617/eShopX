namespace ApplicationCore.UseCases.Carts.AddCartItem;

public record AddCartItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity
) : IRequest<AddCartItemResponse>;