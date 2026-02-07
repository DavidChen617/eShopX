using ApplicationCore.UseCases.Orders;
using Confluent.Kafka;
using eShopX.Common.Extensions;
using Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging;

public class FlashSaleOrderConsumer( 
    IServiceScopeFactory scopeFactory, 
    IOptions<KafkaOptions> options,
    ILogger<FlashSaleOrderConsumer> logger,
    IConsumer<string, string> consumer): BackgroundService
{
    private readonly string _topic = options.Value.FlashSaleOrderTopic;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe(options.Value.FlashSaleOrderTopic);
        logger.LogInformation("Kafka consumer started, listening to {Topic}", _topic);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                await ProcessMessageAsync(result, stoppingToken);
                consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "An error occurred consuming the message");
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        
        consumer.Close();
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken stoppingToken)
    {
        result.Message.Value.TryParseJson<FlashSaleOrderMessage>(out var message, out string _);
        
        if(message is null)
            return;
        
        logger.LogInformation("Processing flash sale order: {UserId} -> {ItemId} x {Qty}",                                                    
            message.UserId, message.FlashSaleItemId, message.Quantity);
        
        using var scope = scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        var orderRepository = provider.GetRequiredService<IRepository<Order>>();
        var flashSaleItemRepository = provider.GetRequiredService<IRepository<FlashSaleItem>>();
        var flashSaleService = provider.GetRequiredService<IFlashSalePurchaseService>();

        try
        {
             // Get flash sale product information                                                                                                                
              var flashSaleItem = await flashSaleItemRepository.GetByIdAsync(message.FlashSaleItemId, stoppingToken);                                       
              if (flashSaleItem is null)                                                                                                         
              {                                                                                                                                  
                  logger.LogWarning("FlashSaleItem not found: {Id}", message.FlashSaleItemId);                                                  
                  await flashSaleService.RollbackStockAsync(message.FlashSaleItemId, message.UserId, message.Quantity);                          
                  return;                                                                                                                        
              }                                                                                                                                  
                                                                                                                                                 
              // Create order                                                                                                                        
              var order = new Order                                                                                                              
              {                                                                                                                                  
                  UserId = message.UserId,                                                                                                       
                  TotalAmount = flashSaleItem.FlashPrice * message.Quantity,                                                                     
                  Status = OrderStatus.Paid,                                                                                                     
                  PaymentMethod = "FlashSale",                                                                                                   
                  PaidAt = DateTime.UtcNow,                                                                                                      
                  ShippingName = "待填寫",                                                                                                       
                  ShippingAddress = "待填寫",                                                                                                    
                  ShippingPhone = "待填寫",                                                                                                      
                  Items = new List<OrderItem>                                                                                                    
                  {                                                                                                                              
                      new()                                                                                                                      
                      {                                                                                                                          
                          ProductId = flashSaleItem.ProductId,                                                                                   
                          ProductName = flashSaleItem.Product?.Name ?? "閃購商品",                                                               
                          Quantity = message.Quantity,                                                                                           
                          UnitPrice = flashSaleItem.FlashPrice                                                                                   
                      }                                                                                                                          
                  }                                                                                                                              
              }; 
              
              await orderRepository.AddAsync(order, stoppingToken);                                                                                         
              await orderRepository.SaveChangesAsync(stoppingToken);                                                                                        
                                                                                                                                                 
              logger.LogInformation("Flash sale order created: {OrderId}", order.Id);
        } 
        catch (Exception ex)                                                                                                                   
        {                                                                                                                                      
            logger.LogError(ex, "Failed to create flash sale order, rolling back stock");                                                     
            await flashSaleService.RollbackStockAsync(message.FlashSaleItemId, message.UserId, message.Quantity);                              
        } 
    }
}
