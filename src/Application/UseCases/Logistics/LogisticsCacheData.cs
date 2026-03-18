namespace Application.UseCases.Logistics;

public record LogisticsCacheData(
    string TempLogisticsID,
    string LogisticsSubType,
    string? ReceiverStoreID,
    string? ReceiverStoreName,
    string? ReceiverAddress,
    string? ReceiverZipCode);
