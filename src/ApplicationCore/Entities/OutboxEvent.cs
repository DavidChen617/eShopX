namespace ApplicationCore.Entities;

public class OutboxEvent: BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public OutboxEventStatus Status { get; set; } = OutboxEventStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? LastError { get; set; }
}

public enum OutboxEventStatus
{
    Pending,
    Processing,
    Processed,
    Failed
}
