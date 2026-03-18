using CoreMesh.Dispatching.Abstractions;

namespace Domain.Aggregates.Shipments.Events;

public sealed record ShipmentCompletedEvent(Guid ShipmentId, Guid OrderId) : INotification;
