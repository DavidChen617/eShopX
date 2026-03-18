using Domain.Outbox;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class OutboxEventRepository(EShopContext db) : IOutboxEventRepository
{
    public Task<List<OutboxEvent>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
        => db.OutboxEvents
            .Where(e => e.Status == OutboxStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
        => await db.OutboxEvents.AddAsync(outboxEvent, cancellationToken);

    public void Update(OutboxEvent outboxEvent)
        => db.OutboxEvents.Update(outboxEvent);
}
