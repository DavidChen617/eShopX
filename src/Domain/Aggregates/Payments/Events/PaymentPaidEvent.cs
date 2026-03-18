using CoreMesh.Dispatching.Abstractions;

namespace Domain.Aggregates.Payments.Events;

public sealed record PaymentPaidEvent(Guid PaymentId, Guid OrderId, string TransactionId) : INotification;
