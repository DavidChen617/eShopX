using CoreMesh.Dispatching.Abstractions;

namespace Domain.Aggregates.Products.Events;

public sealed record ProductUpdatedEvent(Guid ProductId) : INotification;
