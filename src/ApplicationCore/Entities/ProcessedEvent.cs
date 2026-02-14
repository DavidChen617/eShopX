namespace ApplicationCore.Entities;

public class ProcessedEvent: BaseEntity
{
    public string Source { get; set; } = string.Empty; // e.g. "outbox-consumer"
    public Guid EventId { get; set; } 
}
