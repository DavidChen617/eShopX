namespace ApplicationCore.UseCases.Carts.GetCart;

public record GetCartResponse(
    List<CartItemDto> Items,
    decimal TotalAmount,
    int TotalItems
);

public record CartItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,      // Current price (real-time lookup)                                                           
    int Quantity,
    decimal Subtotal,       // UnitPrice * Quantity                                                           
    int StockQuantity,      // Stock quantity                                                                       
    bool InStock            // In stock (StockQuantity >= Quantity)                                          
);