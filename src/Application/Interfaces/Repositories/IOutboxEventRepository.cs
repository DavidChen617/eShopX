using Domain.Outbox;

namespace Application.Interfaces.Repositories;

public interface IOutboxEventRepository
{
    Task<List<OutboxEvent>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);
    void Update(OutboxEvent outboxEvent);
}
