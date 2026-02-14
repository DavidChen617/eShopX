namespace Infrastructure.Messaging;

public record FlashSaleOrderMessage(
    Guid FlashSaleItemId,
    Guid UserId,
    int Quantity,
    DateTime? CreatedAt);
