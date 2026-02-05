namespace ApplicationCore.UseCases.Carts.CheckoutPreview;

public record CheckoutPreviewResponse(
    List<CheckoutPreviewItemDto> Items,
    decimal TotalAmount,
    bool HasUnavailableItems);

public record CheckoutPreviewItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal,
    bool IsAvailable,
    int StockQuantity);