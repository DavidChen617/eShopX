using ApplicationCore.UseCases.Outbox;
using eShopX.Common.Extensions;

namespace Infrastructure.Messaging.Products;

public class ProductIndexOutboxEventHandler(
    IProductSearchIndexSyncService syncService): IOutboxEventHandler
{
    public bool CanHandle(string eventType) =>
        eventType is OutboxEventFactory.ProductUpsertEventType or OutboxEventFactory.ProductDeleteEventType;

    public async Task HandleAsync(OutboxEventEnvelope evt, CancellationToken ct)
    {
        if (!evt.PayloadJson.TryParseJson<ProductOutboxPayload>(out var payload, out var err) || payload is null)
             throw new InvalidOperationException($"Invalid payload: {err}");
        
        switch (evt.EventType)
        {
            case OutboxEventFactory.ProductUpsertEventType:
                await syncService.UpsertProductAsync(payload.ProductId, ct);
                break;
            case OutboxEventFactory.ProductDeleteEventType:
                await syncService.DeleteProductAsync(payload.ProductId, ct);
                break;
        }
    }
}
