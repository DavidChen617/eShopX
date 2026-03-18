using Application.UseCases.Logistics;
using Domain.Aggregates.Shipments;
using Domain.ValueObjects;

namespace Application.Interfaces;

public interface IShipmentFactory
{
    Shipment Create(Guid orderId, LogisticsCacheData logistics, ReceiverInfo receiver);
}
