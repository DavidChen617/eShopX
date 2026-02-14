using Confluent.Kafka;

namespace Infrastructure.Options;

public class KafkaOptions
{
    public static readonly string OptionKey = nameof(KafkaOptions).Substring(0, 5);
    public string FlashSaleOrderTopic { get; set; } = "flash-sale-orders";
    public string ProductIndexTopic { get; set; } = "product-index-events";
    public ConsumerConfig Consumer { get; set; } = new();                                                                                                      
    public ProducerConfig Producer { get; set; } = new(); 
}
