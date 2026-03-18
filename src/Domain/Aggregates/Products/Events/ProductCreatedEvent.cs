using CoreMesh.Dispatching.Abstractions;

namespace Domain.Aggregates.Products.Events;

public sealed record ProductCreatedEvent(Guid ProductId) : INotification;
