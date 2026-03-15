using System.Text.Json;
using eShopX.Domain.Outbox;

namespace eShopX.Application.UseCases.Outbox;

public static class OutboxEventFactory
{
    public const string ProductUpsertEventType = "product.upsert";
    public const string ProductDeleteEventType = "product.delete";

    public static OutboxEvent CreateProductUpsert(Guid productId)
        => OutboxEvent.Create(ProductUpsertEventType, JsonSerializer.Serialize(new ProductOutboxPayload(productId)));

    public static OutboxEvent CreateProductDelete(Guid productId)
        => OutboxEvent.Create(ProductDeleteEventType, JsonSerializer.Serialize(new ProductOutboxPayload(productId)));
}

public record ProductOutboxPayload(Guid ProductId);
