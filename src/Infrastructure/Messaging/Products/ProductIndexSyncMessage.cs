namespace Infrastructure.Messaging.Products;

public record ProductIndexSyncMessage(Guid OutboxEventId, string EventType, string PayloadJson);
