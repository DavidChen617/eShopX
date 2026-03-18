using Domain.Aggregates.Shipments;

namespace Application.Interfaces.Repositories;

public interface IShipmentRepository
{
    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shipment>> GetByOrderIdsAsync(IEnumerable<Guid> orderIds, CancellationToken cancellationToken = default);
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    void Update(Shipment shipment);
    void Delete(Shipment shipment);
}
