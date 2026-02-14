namespace ApplicationCore.Interfaces;

public interface IProcessedEventStore
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid eventId, CancellationToken ct = default);
}
