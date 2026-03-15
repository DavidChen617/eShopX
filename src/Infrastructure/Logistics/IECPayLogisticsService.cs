namespace Infrastructure.Logistics;

public interface IECPayLogisticsService
{
    Task<string> GetSelectionFormHtmlAsync(
        string serverReplyUrl,
        decimal goodsAmount,
        string receiverName,
        string receiverPhone,
        CancellationToken ct = default);

    Task<string> CreateByTempTradeAsync(
        string tempLogisticsId,
        string merchantTradeNo,
        CancellationToken ct = default);

    Task<string> PrintTradeDocumentAsync(
        string logisticsId,
        string logisticsSubType,
        CancellationToken ct = default);

    LogisticsCacheData DecryptClientReply(string resultData);
}
