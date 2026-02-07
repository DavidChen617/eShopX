using Confluent.Kafka;
using eShopX.Common.Extensions;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging;

public interface IFlashSaleOrderPublisher                                                                                                      
{                                                                                                                                              
    Task PublishAsync(FlashSaleOrderMessage message, CancellationToken ct = default);                                                          
}  

public class FlashSaleOrderPublisher(IProducer<string, string> producer, IOptions<KafkaOptions> options): IFlashSaleOrderPublisher, IDisposable
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

    public void Dispose()
    {
        producer.Flush(TimeSpan.FromSeconds(5));
        producer.Dispose();
    }
}
