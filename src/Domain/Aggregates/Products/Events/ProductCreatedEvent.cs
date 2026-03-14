using CoreMesh.Dispatching.Abstractions;

namespace eShopX.Domain.Aggregates.Products.Events;

public sealed record ProductCreatedEvent(Guid ProductId) : INotification;
