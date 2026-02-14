namespace Infrastructure.Messaging;

public record ProductIndexSyncMessage(Guid OutboxEventId, string EventType, string PayloadJson);
