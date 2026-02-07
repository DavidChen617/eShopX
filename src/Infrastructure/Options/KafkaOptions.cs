namespace Infrastructure.Options;

public class KafkaOptions
{
    public static readonly string OptionKey = nameof(KafkaOptions).Substring(0, 5);
    public string BootstrapServers { get; set; } = "localhost:9092";                                                                           
    public string FlashSaleOrderTopic { get; set; } = "flash-sale-orders";                                                                     
    public string GroupId { get; set; } = "eshopx-flash-sale-consumer";
}
