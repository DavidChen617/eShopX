using System.Text.Json;
using Confluent.Kafka;
using eShopX.Domain.Outbox;

namespace Infrastructure.Messaging;

public class OutboxEventPublisher(
    IProducer<string, string> producer,
    IOptions<KafkaOptions> options) : IOutboxEventPublisher
{
    public bool CanHandle(string eventType) => true;

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
