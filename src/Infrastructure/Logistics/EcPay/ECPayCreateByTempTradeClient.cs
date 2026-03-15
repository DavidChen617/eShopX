using System.Text.Json;
using System.Text.Json.Serialization;
using eShopX.Application.Exceptions;

namespace Infrastructure.Logistics.EcPay;

public class EcPayCreateByTempTradeClient(
    EcPayClient client)
{
    public async Task<string> CreateByTempTradeAsync(
        string tempLogisticsId,
        string merchantTradeNo,
        CancellationToken ct = default)
    {
        var innerPayload = JsonSerializer.Serialize(new
        {
            client.Options.MerchantID, 
            TempLogisticsID = tempLogisticsId, 
            MerchantTradeNo = merchantTradeNo
        });

        var requestBody = client.BuildV2Request(innerPayload);

        var text = await client.SendTempTradeCreateAsync(requestBody, ct);

        var outer = JsonSerializer.Deserialize<ECPayV2Response>(text, client.JsonOptions)
                    ?? throw new ExternalServiceException("ECPay", "Invalid CreateByTempTrade response.");

        return client.Decrypt(outer.Data);
    }
}

file record ECPayV2Response([property: JsonPropertyName("Data")] string Data);
