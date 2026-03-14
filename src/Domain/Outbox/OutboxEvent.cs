namespace eShopX.Domain.Outbox;

public sealed class OutboxEvent
{
    private const int MaxRetryCount = 3;

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public OutboxStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private OutboxEvent() { }

    public static OutboxEvent Create(string eventType, string payload)
    {
        return new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            Status = OutboxStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessed()
    {
        Status = OutboxStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed() => Status = OutboxStatus.Failed;

    public void IncrementRetry()
    {
        RetryCount++;
        if (RetryCount >= MaxRetryCount)
            MarkAsFailed();
    }
}
