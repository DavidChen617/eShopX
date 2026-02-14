using ApplicationCore.UseCases.Outbox;
using Confluent.Kafka;
using eShopX.Common.Extensions;

namespace Infrastructure.Messaging.Products;

public class ProductIndexOutboxEventPublisher(
    IProducer<string, string> producer) : IOutboxEventPublisher
{
    private const string Topic = "outbox-events";

    public bool CanHandle(string eventType)
    {
        return eventType is OutboxEventFactory.ProductUpsertEventType or OutboxEventFactory.ProductDeleteEventType;
    }

    public async Task PublishAsync(OutboxEvent @event, CancellationToken ct = default)
    {
        var envelope = new OutboxEventEnvelope(
            @event.Id,
            @event.EventType,
            @event.PayloadJson,
            DateTime.UtcNow);

        var kafkaMessage = new Message<string, string>
        {
            Key = @event.Id.ToString(),
            Value = envelope.ToJson()
        };

        await producer.ProduceAsync(Topic, kafkaMessage, ct);
    }
}
