using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using eShopX.Application.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Logistics;

public class ECPayLogisticsService(
    IHttpClientFactory httpClientFactory,
    IOptions<ECPayOptions> options) : IECPayLogisticsService
{
    private readonly ECPayOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> GetSelectionFormHtmlAsync(
        string serverReplyUrl,
        decimal goodsAmount,
        string receiverName,
        string receiverPhone,
        CancellationToken ct = default)
    {
        var innerPayload = JsonSerializer.Serialize(new
        {
            MerchantID = _options.MerchantID,
            LogisticsType = "CVS",
            LogisticsSubType = string.Empty,
            IsCollection = "N",
            ServerReplyURL = serverReplyUrl,
            GoodsAmount = (int)goodsAmount,
            GoodsName = _options.GoodsName,
            SenderName = _options.SenderName,
            SenderCellPhone = _options.SenderPhone,
            ReceiverName = receiverName,
            ReceiverCellPhone = receiverPhone
        });

        var requestBody = BuildV2Request(innerPayload);
        var http = httpClientFactory.CreateClient();
        var response = await http.PostAsJsonAsync(
            $"{_options.BaseUrl}/Express/v2/RedirectToLogisticsSelection", requestBody, ct);

        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<string> CreateByTempTradeAsync(
        string tempLogisticsId,
        string merchantTradeNo,
        CancellationToken ct = default)
    {
        var innerPayload = JsonSerializer.Serialize(new
        {
            MerchantID = _options.MerchantID,
            TempLogisticsID = tempLogisticsId,
            MerchantTradeNo = merchantTradeNo
        });

        var requestBody = BuildV2Request(innerPayload);
        var http = httpClientFactory.CreateClient();
        var response = await http.PostAsJsonAsync(
            $"{_options.BaseUrl}/Express/v2/CreateByTempTrade", requestBody, ct);

        var text = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException("ECPay", $"CreateByTempTrade failed: {text}");

        var outer = JsonSerializer.Deserialize<ECPayV2Response>(text, JsonOptions)
                    ?? throw new ExternalServiceException("ECPay", "Invalid CreateByTempTrade response.");

        return Decrypt(outer.Data);
    }

    public async Task<string> PrintTradeDocumentAsync(
        string logisticsId,
        string logisticsSubType,
        CancellationToken ct = default)
    {
        var innerPayload = JsonSerializer.Serialize(new
        {
            MerchantID = _options.MerchantID,
            LogisticsID = new[] { logisticsId },
            LogisticsSubType = logisticsSubType
        });

        var requestBody = BuildV2Request(innerPayload);
        var http = httpClientFactory.CreateClient();
        var response = await http.PostAsJsonAsync(
            $"{_options.BaseUrl}/Express/v2/PrintTradeDocument", requestBody, ct);

        return await response.Content.ReadAsStringAsync(ct);
    }

    public LogisticsCacheData DecryptClientReply(string resultData)
    {
        var outer = JsonSerializer.Deserialize<ECPayClientReplyResultData>(resultData, JsonOptions)
                    ?? throw new ExternalServiceException("ECPay", "Invalid client reply ResultData.");

        var decrypted = Decrypt(outer.Data);

        var inner = JsonSerializer.Deserialize<ECPayLogisticsSelectionResult>(decrypted, JsonOptions)
                    ?? throw new ExternalServiceException("ECPay", "Failed to parse decrypted client reply.");

        return new LogisticsCacheData(
            inner.TempLogisticsID,
            inner.LogisticsSubType,
            inner.ReceiverStoreID,
            inner.ReceiverStoreName,
            inner.ReceiverAddress,
            inner.ReceiverZipCode);
    }

    private object BuildV2Request(string innerPayload) => new
    {
        MerchantID = _options.MerchantID,
        RqHeader = new { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
        Data = Encrypt(innerPayload)
    };

    private string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.KeySize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(_options.HashKey);
        aes.IV = Encoding.UTF8.GetBytes(_options.HashIV);

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    private string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.KeySize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(_options.HashKey);
        aes.IV = Encoding.UTF8.GetBytes(_options.HashIV);

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}

file record ECPayV2Response([property: JsonPropertyName("Data")] string Data);

file record ECPayClientReplyResultData([property: JsonPropertyName("Data")] string Data);

file record ECPayLogisticsSelectionResult(
    string TempLogisticsID,
    string LogisticsSubType,
    string? ReceiverStoreID,
    string? ReceiverStoreName,
    string? ReceiverAddress,
    string? ReceiverZipCode);
