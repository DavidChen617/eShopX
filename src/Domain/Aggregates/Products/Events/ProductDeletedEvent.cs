using CoreMesh.Dispatching.Abstractions;

namespace eShopX.Domain.Aggregates.Products.Events;

public sealed record ProductDeletedEvent(Guid ProductId) : INotification;
