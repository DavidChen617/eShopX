using eShopX.Domain.Aggregates.Shipments.Events;

namespace eShopX.Domain.Aggregates.Shipments;

public abstract class Shipment : AggregateRoot
{
    public Guid OrderId { get; protected set; }
    public string LogisticsId { get; protected set; } = default!;
    public LogisticsSubType LogisticsSubType { get; protected set; }
    public string LogisticsStatus { get; protected set; } = default!;
    public string LogisticsStatusName { get; protected set; } = default!;
    public DateTime UpdateStatusDate { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    public void UpdateStatus(string logisticsStatus, string logisticsStatusName, DateTime updateStatusDate)
    {
        LogisticsStatus = logisticsStatus;
        LogisticsStatusName = logisticsStatusName;
        UpdateStatusDate = updateStatusDate;
    }

    public void MarkAsCompleted()
    {
        RaiseDomainEvent(new ShipmentCompletedEvent(Id, OrderId));
    }
}
