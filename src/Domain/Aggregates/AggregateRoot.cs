using CoreMesh.Dispatching.Abstractions;

namespace eShopX.Domain.Aggregates;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }

    private readonly List<INotification> _domainEvents = [];

    public IReadOnlyList<INotification> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(INotification domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}
