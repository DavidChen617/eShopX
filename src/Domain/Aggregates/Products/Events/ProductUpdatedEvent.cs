using CoreMesh.Dispatching.Abstractions;

namespace eShopX.Domain.Aggregates.Products.Events;

public sealed record ProductUpdatedEvent(Guid ProductId) : INotification;
