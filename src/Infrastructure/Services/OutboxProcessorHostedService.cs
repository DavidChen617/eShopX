using ApplicationCore.UseCases.Outbox;
using eShopX.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OutboxProcessorHostedService(
    ILogger<OutboxProcessorHostedService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
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
                logger.LogError(ex, "Error processing outbox events");
            }
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        var outboxEventRepository = provider.GetRequiredService<IRepository<OutboxEvent>>();
        var syncService = scope.ServiceProvider.GetRequiredService<IProductSearchIndexSyncService>();
        var now = DateTime.UtcNow;
        var processingExpiredAt = now - ProcessingTimeout;
      
        var events = await outboxEventRepository.QueryAsync(q =>
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
        
        foreach (var e in events)
        {
            e.Status = OutboxEventStatus.Processing;
            e.NextRetryAt = DateTime.UtcNow;
            outboxEventRepository.Update(e);
        }
        
        await outboxEventRepository.SaveChangesAsync(stoppingToken);
        
        foreach (var e in events)
        {
            outboxEventRepository.Update(e);
            
            if (stoppingToken.IsCancellationRequested)
            {
                e.Status = OutboxEventStatus.Pending;
                e.NextRetryAt = DateTime.UtcNow.AddSeconds(10);
                outboxEventRepository.Update(e);
                continue;
            }
            
            try
            {
                await HandleEventAsync(e, syncService, stoppingToken);
                e.Status = OutboxEventStatus.Processed;
                e.ProcessedAt = DateTime.UtcNow;
                e.LastError = null;
                e.NextRetryAt = null;
            }
            catch (Exception ex)
            {
                e.RetryCount += 1;
                e.Status = OutboxEventStatus.Pending;
                e.LastError = ex.Message;
                e.NextRetryAt = DateTime.UtcNow.Add(GetBackoff(e.RetryCount));
                
                logger.LogWarning(ex,
                    "Outbox event failed. Id={OutboxId}, Type={EventType}, Retry={RetryCount}",
                    e.Id, e.EventType, e.RetryCount);
            }
            outboxEventRepository.Update(e);
        }
        await outboxEventRepository.SaveChangesAsync(stoppingToken);
    }

    private static async Task HandleEventAsync(
        OutboxEvent evt,
        IProductSearchIndexSyncService syncService,
        CancellationToken ct)
    {
        if (!evt.PayloadJson.TryParseJson<ProductOutboxPayload>(out var payload, out var errMsg))
        {
            throw new InvalidOperationException($"Invalid outbox payload. OutboxId={evt.Id}, error={errMsg}");
        }

        switch (evt.EventType)
        {
            case OutboxEventFactory.ProductUpsertEventType:
                await syncService.UpsertProductAsync(payload!.ProductId, ct);
                break;

            case OutboxEventFactory.ProductDeleteEventType:
                await syncService.DeleteProductAsync(payload!.ProductId, ct);
                break;

            default:
                throw new InvalidOperationException($"Unknown event type: {evt.EventType}");
        }
    }

    private static TimeSpan GetBackoff(int retryCount)
    {
        var minutes = Math.Min(30, (int)Math.Pow(2, Math.Clamp(retryCount - 1, 0, 5)));
        return TimeSpan.FromMinutes(Math.Max(1, minutes));
    }
}
