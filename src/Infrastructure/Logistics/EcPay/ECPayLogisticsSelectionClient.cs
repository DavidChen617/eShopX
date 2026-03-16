using System.Text.Json;
using System.Text.Json.Serialization;
using eShopX.Application.Exceptions;
using eShopX.Application.UseCases.Logistics;

namespace Infrastructure.Logistics.EcPay;

public class EcPayLogisticsSelectionClient(
    EcPayClient client)
{

    public async Task<string> GetSelectionFormHtmlAsync(
        string serverReplyUrl,
        string clientReplyUrl,
        decimal goodsAmount,
        string receiverName,
        string receiverPhone,
        CancellationToken ct = default)
    {
        var innerPayload = JsonSerializer.Serialize(new
        {
            client.Options.MerchantID,
            LogisticsType = "CVS",
            LogisticsSubType = string.Empty,
            IsCollection = "N",
            ServerReplyURL = serverReplyUrl,
            ClientReplyURL = clientReplyUrl,
            GoodsAmount = (int)goodsAmount,
            GoodsName = client.Options.GoodsName,
            SenderName = client.Options.SenderName,
            SenderCellPhone = client.Options.SenderPhone,
            SenderZipCode = client.Options.SenderZipCode,
            SenderAddress = client.Options.SenderAddress,
            ReceiverName = receiverName,
            ReceiverCellPhone = receiverPhone
        });

        var requestBody = client.BuildV2Request(innerPayload);
        return await client.SendSelectionFormHtmlGettingAsync(requestBody, ct);
    }

    public LogisticsCacheData DecryptClientReply(string resultData)
    {
        var decoded = Uri.UnescapeDataString(resultData);
        var outer = JsonSerializer.Deserialize<ECPayClientReplyResultData>(decoded, client.JsonOptions)
                    ?? throw new ExternalServiceException("ECPay", "Invalid client reply ResultData.");

        var decrypted = Uri.UnescapeDataString(client.Decrypt(outer.Data));

        var inner = JsonSerializer.Deserialize<ECPayLogisticsSelectionResult>(decrypted, client.JsonOptions)
                    ?? throw new ExternalServiceException("ECPay", "Failed to parse decrypted client reply.");

        return new LogisticsCacheData(
            inner.TempLogisticsID,
            inner.LogisticsSubType,
            inner.ReceiverStoreID,
            inner.ReceiverStoreName,
            inner.ReceiverAddress,
            inner.ReceiverZipCode);
    }
}

file record ECPayClientReplyResultData([property: JsonPropertyName("Data")] string Data);

file record ECPayLogisticsSelectionResult(
    string TempLogisticsID,
    string LogisticsSubType,
    string? ReceiverStoreID,
    string? ReceiverStoreName,
    string? ReceiverAddress,
    string? ReceiverZipCode);
