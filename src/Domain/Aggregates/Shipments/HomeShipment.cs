using Domain.ValueObjects;

namespace Domain.Aggregates.Shipments;

public sealed class HomeShipment : Shipment
{
    public ReceiverInfo Receiver { get; private set; } = default!;
    public string ZipCode { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public string? BookingNote { get; private set; }

    private HomeShipment() { }

    public static HomeShipment Create(Guid orderId, string logisticsId, LogisticsSubType logisticsSubType,
        ReceiverInfo receiver, string zipCode, string address, string? bookingNote)
    {
        return new HomeShipment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            LogisticsId = logisticsId,
            LogisticsSubType = logisticsSubType,
            LogisticsStatus = string.Empty,
            LogisticsStatusName = string.Empty,
            UpdateStatusDate = DateTime.UtcNow,
            Receiver = receiver,
            ZipCode = zipCode,
            Address = address,
            BookingNote = bookingNote,
            CreatedAt = DateTime.UtcNow
        };
    }
}
