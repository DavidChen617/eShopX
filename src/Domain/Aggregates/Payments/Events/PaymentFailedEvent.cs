using CoreMesh.Dispatching.Abstractions;

namespace Domain.Aggregates.Payments.Events;

public sealed record PaymentFailedEvent(Guid PaymentId, Guid OrderId) : INotification;
