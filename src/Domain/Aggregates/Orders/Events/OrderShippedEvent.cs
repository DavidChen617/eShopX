using CoreMesh.Dispatching.Abstractions;

namespace Domain.Aggregates.Orders.Events;

public sealed record OrderShippedEvent(Guid OrderId, Guid UserId) : INotification;
