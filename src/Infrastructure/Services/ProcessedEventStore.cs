namespace Infrastructure.Services;

public class ProcessedEventStore(
    IRepository<ProcessedEvent> processedEventsRepository)
    : IProcessedEventStore
{
    private const string Source = "outbox-consumer";

    public async Task<bool> ExistsAsync(Guid eventId, CancellationToken ct = default)
    {
        var item = await processedEventsRepository.QueryAsync(
            q =>
                q.Where(e => e.Source == Source && e.EventId == eventId).Take(1), ct);

        return item.Count > 0;
    }

    public async Task MarkProcessedAsync(Guid eventId, CancellationToken ct = default)
    {
        var entity = new ProcessedEvent { EventId = eventId, Source = Source, };
        await processedEventsRepository.AddAsync(entity, ct);

        try
        {
            await processedEventsRepository.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex.Message.Contains("IX_ProcessedEvents_Source_EventId",
                                       StringComparison.OrdinalIgnoreCase))
        {
            // duplicated event, ignore
        }
    }
}
