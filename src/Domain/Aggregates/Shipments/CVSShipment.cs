using Domain.ValueObjects;

namespace Domain.Aggregates.Shipments;

public sealed class CVSShipment : Shipment
{
    public ReceiverInfo Receiver { get; private set; } = default!;
    public string StoreId { get; private set; } = default!;
    public string StoreName { get; private set; } = default!;
    public string? CVSPaymentNo { get; private set; }

    private CVSShipment() { }

    public static CVSShipment Create(Guid orderId, string logisticsId, LogisticsSubType logisticsSubType,
        ReceiverInfo receiver, string storeId, string storeName)
    {
        return new CVSShipment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            LogisticsId = logisticsId,
            LogisticsSubType = logisticsSubType,
            LogisticsStatus = string.Empty,
            LogisticsStatusName = string.Empty,
            UpdateStatusDate = DateTime.UtcNow,
            Receiver = receiver,
            StoreId = storeId,
            StoreName = storeName,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateCVSPaymentNo(string cvsPaymentNo) => CVSPaymentNo = cvsPaymentNo;
}
