namespace eShopX.Domain.Aggregates.Shipments;

public sealed class HomeShipment : Shipment
{
    public string ReceiverName { get; private set; } = default!;
    public string ReceiverCellPhone { get; private set; } = default!;
    public string ZipCode { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public string? BookingNote { get; private set; }

    private HomeShipment() { }

    public static HomeShipment Create(Guid orderId, string logisticsId, LogisticsSubType logisticsSubType,
        string receiverName, string receiverCellPhone, string zipCode, string address, string? bookingNote)
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
            ReceiverName = receiverName,
            ReceiverCellPhone = receiverCellPhone,
            ZipCode = zipCode,
            Address = address,
            BookingNote = bookingNote,
            CreatedAt = DateTime.UtcNow
        };
    }
}
