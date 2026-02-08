namespace ApplicationCore.UseCases.Homepage.GetFlashSale;

public record GetFlashSaleResponse(
    Guid Id,
    string Title,
    string? Subtitle,
    DateTime StartsAt,
    DateTime EndsAt,
    List<FlashSaleSlotItem> Slots,
    List<FlashSaleProductItem> Items);

public record FlashSaleSlotItem(
    Guid Id,
    string Label,
    DateTime StartsAt,
    DateTime EndsAt,
    string Status);

public record FlashSaleProductItem(
    Guid FlashSaleItemId,
    Guid ProductId,
    string Name,
    string? ImageUrl,
    decimal Price,
    decimal OriginalPrice,
    int StockTotal,
    int StockRemaining,
    string? Badge);
