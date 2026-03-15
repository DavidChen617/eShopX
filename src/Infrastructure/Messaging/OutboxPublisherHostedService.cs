using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

public class OutboxPublisherHostedService(
    ILogger<OutboxPublisherHostedService> logger,
    IServiceScopeFactory scopeFactory,
    IEnumerable<IOutboxEventPublisher> publishers) : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error publishing outbox events");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var events = await outboxRepository.GetPendingAsync(BatchSize, stoppingToken);
        if (events.Count == 0) return;

        foreach (var evt in events)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                var publisher = publishers.FirstOrDefault(x => x.CanHandle(evt.EventType));
                if (publisher is null)
                {
                    evt.MarkAsFailed();
                    logger.LogWarning("No outbox publisher for event type: {EventType}", evt.EventType);
                }
                else
                {
                    await publisher.PublishAsync(evt, stoppingToken);
                    evt.MarkAsProcessed();
                }
            }
            catch (Exception ex)
            {
                evt.IncrementRetry();
                logger.LogWarning(ex,
                    "Outbox publish failed. Id={OutboxId}, Type={EventType}",
                    evt.Id, evt.EventType);
            }

            outboxRepository.Update(evt);
        }

        await unitOfWork.SaveChangesAsync(stoppingToken);
    }
}
