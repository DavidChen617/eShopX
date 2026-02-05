namespace ApplicationCore.UseCases.Carts.AddCartItem;

public record AddCartItemResponse(
    Guid CartId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);