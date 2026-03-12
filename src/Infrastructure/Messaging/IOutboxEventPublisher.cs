namespace Infrastructure.Messaging;

public interface IOutboxEventPublisher
{
    bool CanHandle(string eventType);
    Task PublishAsync(OutboxEvent @event, CancellationToken ct = default);
}
