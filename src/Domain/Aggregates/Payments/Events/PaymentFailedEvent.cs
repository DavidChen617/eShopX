using CoreMesh.Dispatching.Abstractions;

namespace eShopX.Domain.Aggregates.Payments.Events;

public sealed record PaymentFailedEvent(Guid PaymentId, Guid OrderId) : INotification;
