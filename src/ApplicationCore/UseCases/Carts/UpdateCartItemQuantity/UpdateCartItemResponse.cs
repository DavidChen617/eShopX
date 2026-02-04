namespace ApplicationCore.UseCases.Carts.UpdateCartItemQuantity;

public record UpdateCartItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);
