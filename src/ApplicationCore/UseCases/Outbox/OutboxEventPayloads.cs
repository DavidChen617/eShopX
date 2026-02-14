namespace ApplicationCore.UseCases.Outbox;

public record ProductOutboxPayload(Guid ProductId);
public record FlashSaleOrderOutboxPayload(Guid UserId, Guid FlashSaleItemId, int Quantity);
