using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Logistics.EcPay;

public class EcPayClient(HttpClient httpClient, IOptions<EcPayOptions> options)
{
    internal readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    internal readonly JsonSerializerOptions RequestJsonOptions = new();  // PascalCase, no naming policy
    internal readonly EcPayOptions Options = options.Value;
    
    internal object BuildV2Request(string innerPayload) => new
    {
        Options.MerchantID,
        RqHeader = new { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
        Data = Encrypt(innerPayload)
    };

    internal async Task<string> SendSelectionFormHtmlGettingAsync(object payload, CancellationToken ct)
    {
        var response = await httpClient.PostAsJsonAsync("RedirectToLogisticsSelection", payload, RequestJsonOptions, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    internal async Task<string> SendTempTradeCreateAsync(object payload, CancellationToken ct)
    {
        var response = await httpClient.PostAsJsonAsync("CreateByTempTrade", payload, RequestJsonOptions, ct);
        var text = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException("ECPay", $"CreateByTempTrade failed: {text}");

        return text;
    }

    internal async Task<string> SendTradeDocumentPrintingAsync(object payload, CancellationToken ct)
    {
        var response = await httpClient.PostAsJsonAsync("PrintTradeDocument", payload, RequestJsonOptions, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    internal string Encrypt(string plainText)
    {
        using var aes = BuildAes();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    internal string Decrypt(string cipherText)
    {
        using var aes = BuildAes();
        
        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private Aes BuildAes()
    {
        var aes = Aes.Create();
        aes.KeySize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(Options.HashKey);
        aes.IV = Encoding.UTF8.GetBytes(Options.HashIV);
        return aes;
    }
}
