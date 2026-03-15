using System.Text.Json;
using Confluent.Kafka;
using eShopX.Application.UseCases.Outbox;
using eShopX.Domain.Outbox;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging.Products;

public class ProductIndexOutboxEventPublisher(
    IProducer<string, string> producer,
    IOptions<KafkaOptions> options) : IOutboxEventPublisher
{
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

        await producer.ProduceAsync(options.Value.OutboxEventTopic, kafkaMessage, ct);
    }
}
