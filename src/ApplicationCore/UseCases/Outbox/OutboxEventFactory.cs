using eShopX.Common.Extensions;

namespace ApplicationCore.UseCases.Outbox;

public static class OutboxEventFactory
{
    public const string ProductUpsertEventType = "product.upsert";
    public const string ProductDeleteEventType = "product.delete";
    public const string FlashSaleOrderEventType = "flash-sale.order";

    public static OutboxEvent CreateProductUpsert(Guid productId) =>
        new()
        {
            EventType = ProductUpsertEventType,
            PayloadJson = (new ProductOutboxPayload(productId)).ToJson(),
            Status = OutboxEventStatus.Pending,
            RetryCount = 0,
            NextRetryAt = DateTime.UtcNow
        };

    public static OutboxEvent CreateProductDelete(Guid productId) =>
        new()
        {
            EventType = ProductDeleteEventType,
            PayloadJson = (new ProductOutboxPayload(productId)).ToJson(),
            Status = OutboxEventStatus.Pending,
            RetryCount = 0,
            NextRetryAt = DateTime.UtcNow
        };

    public static OutboxEvent CreateFlashSaleOrder(Guid userId, Guid flashSaleItemId, int quantity) =>
        new()
        {
            EventType = FlashSaleOrderEventType,
            PayloadJson = (new FlashSaleOrderOutboxPayload(userId, flashSaleItemId, quantity)).ToJson(),
            Status = OutboxEventStatus.Pending,
            RetryCount = 0,
            NextRetryAt = DateTime.UtcNow
        };
}
