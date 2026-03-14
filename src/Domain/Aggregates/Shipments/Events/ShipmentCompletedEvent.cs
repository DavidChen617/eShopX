using CoreMesh.Dispatching.Abstractions;

namespace eShopX.Domain.Aggregates.Shipments.Events;

public sealed record ShipmentCompletedEvent(Guid ShipmentId, Guid OrderId) : INotification;
