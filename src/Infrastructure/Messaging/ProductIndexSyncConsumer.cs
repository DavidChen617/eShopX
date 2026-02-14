using ApplicationCore.UseCases.Outbox;
using Confluent.Kafka;
using eShopX.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging;

public class ProductIndexSyncConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<ProductIndexSyncConsumer> logger,
    IConsumer<string, string> consumer) : BackgroundService
{
    private readonly string _topic = options.Value.ProductIndexTopic;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe(_topic);
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
                logger.LogError(ex, "An error occurred consuming product index message");
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
        if (!result.Message.Value.TryParseJson<ProductIndexSyncMessage>(out var message, out var error))
        {
            logger.LogWarning("Invalid product index message payload: {Error}", error);
            return;
        }

        if (!message!.PayloadJson.TryParseJson<ProductOutboxPayload>(out var payload, out error))
        {
            logger.LogWarning("Invalid product index payload. OutboxId={OutboxId}, Error={Error}", message.OutboxEventId, error);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var syncService = scope.ServiceProvider.GetRequiredService<IProductSearchIndexSyncService>();

        switch (message.EventType)
        {
            case OutboxEventFactory.ProductUpsertEventType:
                await syncService.UpsertProductAsync(payload!.ProductId, stoppingToken);
                break;
            case OutboxEventFactory.ProductDeleteEventType:
                await syncService.DeleteProductAsync(payload!.ProductId, stoppingToken);
                break;
            default:
                logger.LogWarning("Unknown product index event type. OutboxId={OutboxId}, EventType={EventType}",
                    message.OutboxEventId, message.EventType);
                break;
        }
    }
}
