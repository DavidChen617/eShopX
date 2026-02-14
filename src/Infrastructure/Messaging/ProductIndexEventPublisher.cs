using ApplicationCore.UseCases.Outbox;
using Confluent.Kafka;
using eShopX.Common.Extensions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging;

public interface IOutboxEventPublisher
{
    bool CanHandle(string eventType);
    Task PublishAsync(OutboxEvent @event, CancellationToken ct = default);
}

public class ProductIndexOutboxEventPublisher(
    IProducer<string, string> producer,
    IOptions<KafkaOptions> options) : IOutboxEventPublisher
{
    private readonly string _topic = options.Value.ProductIndexTopic;

    public bool CanHandle(string eventType)
    {
        return eventType is OutboxEventFactory.ProductUpsertEventType or OutboxEventFactory.ProductDeleteEventType;
    }

    public async Task PublishAsync(OutboxEvent @event, CancellationToken ct = default)
    {
        var message = new ProductIndexSyncMessage(@event.Id, @event.EventType, @event.PayloadJson);
        var kafkaMessage = new Message<string, string>
        {
            Key = @event.Id.ToString(),
            Value = message.ToJson()
        };

        await producer.ProduceAsync(_topic, kafkaMessage, ct);
    }
}
