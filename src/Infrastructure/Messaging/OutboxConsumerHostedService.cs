using Confluent.Kafka;
using eShopX.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

public class OutboxConsumerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxConsumerHostedService> logger) : BackgroundService
{
    private const string Topic = "outbox-events";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<string, string>>();
        consumer.Subscribe(Topic);
        logger.LogInformation("Outbox consumer started, listening to {Topic}", Topic);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                
                if (!result.Message.Value.TryParseJson<OutboxEventEnvelope>(out var outboxEvent, out var errorMessage) ||
                    outboxEvent is null)
                {
                    logger.LogWarning("Invalid outbox message. Error={Error}", errorMessage);
                    consumer.Commit(result);
                    continue;
                }

                var processedEventStore = scope.ServiceProvider.GetRequiredService<IProcessedEventStore>();
        
                if (await processedEventStore.ExistsAsync(outboxEvent.EventId, stoppingToken))
                {
                    consumer.Commit(result);
                    continue;
                }

                var handlers = scope.ServiceProvider.GetServices<IOutboxEventHandler>();
                var handler = handlers.FirstOrDefault(h => h.CanHandle(outboxEvent.EventType));
                if (handler is null)
                {
                    logger.LogWarning("No handler for outbox event type: {EventType}", outboxEvent.EventType);
                    consumer.Commit(result);
                    continue;
                }

                await handler.HandleAsync(outboxEvent, stoppingToken);
                await processedEventStore.MarkProcessedAsync(outboxEvent.EventId, stoppingToken);
                consumer.Commit(result);
            } catch (ConsumeException ex)
            {
                logger.LogError(ex, "Kafka consume error");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // do not commit; let Kafka redeliver
                logger.LogError(ex, "Failed to process outbox event");
            }
        }
        consumer.Close();
    }
}
