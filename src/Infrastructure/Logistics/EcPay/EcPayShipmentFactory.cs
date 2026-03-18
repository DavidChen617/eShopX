using Application.Interfaces;
using Application.UseCases.Logistics;
using Domain.Aggregates.Shipments;
using Domain.ValueObjects;

namespace Infrastructure.Logistics.EcPay;

public class EcPayShipmentFactory : IShipmentFactory
{
    private static readonly HashSet<LogisticsSubType> CvsTypes =
        [LogisticsSubType.UNIMART, LogisticsSubType.FAMI, LogisticsSubType.HILIFE];

    public Shipment Create(Guid orderId, LogisticsCacheData logistics, ReceiverInfo receiver)
    {
        if (!Enum.TryParse<LogisticsSubType>(logistics.LogisticsSubType, true, out var subType))
            throw new ArgumentException($"Invalid LogisticsSubType: {logistics.LogisticsSubType}");

        return CvsTypes.Contains(subType)
            ? CVSShipment.Create(orderId, logistics.TempLogisticsID, subType,
                receiver, logistics.ReceiverStoreID!, logistics.ReceiverStoreName!)
            : HomeShipment.Create(orderId, logistics.TempLogisticsID, subType,
                receiver, logistics.ReceiverZipCode!, logistics.ReceiverAddress!, null);
    }
}
