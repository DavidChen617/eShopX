namespace eShopX.Common.Logging;

public interface ILogDispatcher
{
    bool TryEnqueue(LogEntry entry);
    ValueTask EnqueueAsync(LogEntry entry, CancellationToken cancellationToken);
    Task ProcessAsync(CancellationToken cancellationToken);
    Task Completion { get; }
    void Complete();
}