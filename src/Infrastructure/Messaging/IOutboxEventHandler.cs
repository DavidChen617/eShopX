namespace Infrastructure.Messaging;

public interface IOutboxEventHandler
{
    bool CanHandle(string eventType);
    Task HandleAsync(OutboxEventEnvelope evt, CancellationToken ct);    
}
