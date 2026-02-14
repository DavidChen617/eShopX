namespace Infrastructure.Messaging;

public record OutboxEventEnvelope(Guid EventId, string EventType, string PayloadJson, DateTime OccurredAtUtc);
