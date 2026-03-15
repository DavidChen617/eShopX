using System.Text.Json;
namespace Infrastructure.Logistics.EcPay;

public class EcPayPrintTradeDocumentClient(
    EcPayClient client)
{
    public async Task<string> PrintTradeDocumentAsync(
        string logisticsId,
        string logisticsSubType,
        CancellationToken ct = default)
    {
        var innerPayload = JsonSerializer.Serialize(new
        {
            client.Options.MerchantID,
            LogisticsID = new[] { logisticsId },
            LogisticsSubType = logisticsSubType
        });

        var requestBody = client.BuildV2Request(innerPayload);

        return await client.SendTradeDocumentPrintingAsync(requestBody, ct);
    }
}
