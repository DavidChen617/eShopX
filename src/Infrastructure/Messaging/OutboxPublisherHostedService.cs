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
    private static readonly TimeSpan ProcessingTimeout = TimeSpan.FromMinutes(5);

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
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IRepository<OutboxEvent>>();

        var now = DateTime.UtcNow;
        var processingExpiredAt = now - ProcessingTimeout;

        var events = await outboxRepository.QueryAsync(q =>
                q.Where(x =>
                        (x.Status == OutboxEventStatus.Pending &&
                         (x.NextRetryAt == null || x.NextRetryAt <= now)) ||
                        (x.Status == OutboxEventStatus.Processing &&
                         x.NextRetryAt != null &&
                         x.NextRetryAt <= processingExpiredAt))
                    .OrderBy(x => x.CreatedAt)
                    .Take(BatchSize),
            stoppingToken);

        if (events.Count == 0)
            return;

        foreach (var evt in events)
        {
            evt.Status = OutboxEventStatus.Processing;
            evt.NextRetryAt = DateTime.UtcNow;
            outboxRepository.Update(evt);
        }

        await outboxRepository.SaveChangesAsync(stoppingToken);

        foreach (var evt in events)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                evt.Status = OutboxEventStatus.Pending;
                evt.NextRetryAt = DateTime.UtcNow.AddSeconds(10);
                outboxRepository.Update(evt);
                continue;
            }

            try
            {
                var publisher = publishers.FirstOrDefault(x => x.CanHandle(evt.EventType));
                if (publisher is null)
                {
                    throw new InvalidOperationException($"No outbox publisher for event type: {evt.EventType}");
                }
                await publisher.PublishAsync(evt, stoppingToken);

                evt.Status = OutboxEventStatus.Processed;
                evt.ProcessedAt = DateTime.UtcNow;
                evt.LastError = null;
                evt.NextRetryAt = null;
            }
            catch (Exception ex)
            {
                evt.RetryCount += 1;
                evt.Status = OutboxEventStatus.Pending;
                evt.LastError = ex.Message;
                evt.NextRetryAt = DateTime.UtcNow.Add(GetBackoff(evt.RetryCount));

                logger.LogWarning(ex,
                    "Outbox publish failed. Id={OutboxId}, Type={EventType}, Retry={RetryCount}",
                    evt.Id, evt.EventType, evt.RetryCount);
            }

            outboxRepository.Update(evt);
        }

        await outboxRepository.SaveChangesAsync(stoppingToken);
    }

    private static TimeSpan GetBackoff(int retryCount)
    {
        var minutes = Math.Min(30, (int)Math.Pow(2, Math.Clamp(retryCount - 1, 0, 5)));
        return TimeSpan.FromMinutes(Math.Max(1, minutes));
    }
}
