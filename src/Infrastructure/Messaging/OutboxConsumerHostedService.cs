using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Messaging;

public class OutboxConsumerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxConsumerHostedService> logger) : BackgroundService
{
    private const string Topic = "outbox-events";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = scopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<IConsumer<string, string>>();
        consumer.Subscribe(Topic);
        logger.LogInformation("Outbox consumer started, listening to {Topic}", Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);

                OutboxEventEnvelope? outboxEvent;
                try
                {
                    outboxEvent = JsonSerializer.Deserialize<OutboxEventEnvelope>(result.Message.Value);
                }
                catch (JsonException ex)
                {
                    logger.LogWarning("Invalid outbox message: {Error}", ex.Message);
                    consumer.Commit(result);
                    continue;
                }

                if (outboxEvent is null)
                {
                    consumer.Commit(result);
                    continue;
                }

                using var messageScope = scopeFactory.CreateScope();
                var handlers = messageScope.ServiceProvider.GetServices<IOutboxEventHandler>();
                var handler = handlers.FirstOrDefault(h => h.CanHandle(outboxEvent.EventType));
                if (handler is null)
                {
                    logger.LogWarning("No handler for outbox event type: {EventType}", outboxEvent.EventType);
                    consumer.Commit(result);
                    continue;
                }

                await handler.HandleAsync(outboxEvent, stoppingToken);
                consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Kafka consume error");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox event");
            }
        }

        consumer.Close();
    }
}
