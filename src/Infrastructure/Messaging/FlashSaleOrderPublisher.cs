using ApplicationCore.UseCases.Outbox;
using Confluent.Kafka;
using eShopX.Common.Exceptions;
using eShopX.Common.Extensions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging;

public interface IFlashSaleOrderPublisher                                                                                                      
{                                                                                                                                              
    Task PublishAsync(FlashSaleOrderMessage message, CancellationToken ct = default);                                                          
}  

public class FlashSaleOrderPublisher(
    IProducer<string, string> producer,
    IOptions<KafkaOptions> options): IFlashSaleOrderPublisher, IOutboxEventPublisher
{
    private readonly string _topic = options.Value.FlashSaleOrderTopic;
    public async Task PublishAsync(FlashSaleOrderMessage message, CancellationToken ct = default)
    {
        var json = message.ToJson();
        
        var kafkaMessage = new Message<string, string>
        {
            Key = message.UserId.ToString(),
            Value = json
        };
        
        await producer.ProduceAsync(_topic, kafkaMessage, ct);
    }

    public bool CanHandle(string eventType)
    {
        return eventType == OutboxEventFactory.FlashSaleOrderEventType;
    }

    public async Task PublishAsync(OutboxEvent @event, CancellationToken ct = default)
    {
        if (!@event.PayloadJson.TryParseJson<FlashSaleOrderOutboxPayload>(out var payload, out var error))
        {
            throw new ValidationException("outbox.payload", $"Invalid flash sale payload: {error}");
        }

        await PublishAsync(new FlashSaleOrderMessage(
            payload!.UserId,
            payload.FlashSaleItemId,
            payload.Quantity,
            DateTime.UtcNow), ct);
    }
}
