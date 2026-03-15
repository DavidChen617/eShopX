using System.Text.Json;
using Confluent.Kafka;
using eShopX.Application.UseCases.Outbox;
using eShopX.Domain.Outbox;

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
            @event.Payload,
            DateTime.UtcNow);

        var kafkaMessage = new Message<string, string>
        {
            Key = @event.Id.ToString(),
            Value = JsonSerializer.Serialize(envelope)
        };

        await producer.ProduceAsync(Topic, kafkaMessage, ct);
    }
}
