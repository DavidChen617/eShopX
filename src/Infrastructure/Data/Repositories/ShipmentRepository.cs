using Domain.Aggregates.Shipments;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class ShipmentRepository(EShopContext db) : IShipmentRepository
{
    public Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => db.Shipments.FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
        => await db.Shipments.AddAsync(shipment, cancellationToken);

    public void Update(Shipment shipment)
        => db.Shipments.Update(shipment);
}
