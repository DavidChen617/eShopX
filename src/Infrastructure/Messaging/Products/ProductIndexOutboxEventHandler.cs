using System.Text.Json;

namespace Infrastructure.Messaging.Products;

public class ProductIndexOutboxEventHandler(
    IProductSearchIndexSynchronizer synchronizer) : IOutboxEventHandler
{
    public bool CanHandle(string eventType) =>
        eventType is OutboxEventFactory.ProductUpsertEventType or OutboxEventFactory.ProductDeleteEventType;

    public async Task HandleAsync(OutboxEventEnvelope evt, CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<ProductOutboxPayload>(evt.PayloadJson)
                      ?? throw new InvalidOperationException($"Invalid payload for event {evt.EventId}");

        switch (evt.EventType)
        {
            case OutboxEventFactory.ProductUpsertEventType:
                await synchronizer.UpsertProductAsync(payload.ProductId, ct);
                break;
            case OutboxEventFactory.ProductDeleteEventType:
                await synchronizer.DeleteProductAsync(payload.ProductId, ct);
                break;
        }
    }
}
