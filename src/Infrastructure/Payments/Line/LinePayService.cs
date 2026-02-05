using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using eShopX.Common.Extensions;

using Infrastructure.Options;
using Infrastructure.Payments.Line.Models;

using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.Line;

public class LinePayService(IHttpClientFactory httpClientFactory, IOptions<LinePayOptions> options)
    : IPaymentService<LinePayRequest, LinePayRequestResponse?, LinePayConfirmInput, LinePayConfirmResponse?>
{
    private readonly LinePayOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<LinePayRequestResponse?> RequestPaymentAsync(LinePayRequest request, CancellationToken ct = default)
    {
        var path = "/v3/payments/request";
        var normalizedRequest = request with
        {
            RedirectUrls = NormalizeRedirectUrls(request.RedirectUrls)
        };
        var body = JsonSerializer.Serialize(normalizedRequest, JsonOptions);
        var text = await SendAsync("POST", path, body, ct);
        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }
        text.TryParseJson<LinePayRequestResponse>(out var result, out var msg, JsonOptions);
        return result;
    }

    public async Task<LinePayConfirmResponse?> ConfirmPaymentAsync(long transactionId, LinePayConfirmRequest request,
        CancellationToken ct = default)
    {
        var path = $"/v3/payments/{transactionId}/confirm";
        var body = JsonSerializer.Serialize(request, JsonOptions);
        var text = await SendAsync("POST", path, body, ct);
        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }
        text.TryParseJson<LinePayConfirmResponse>(out var result, out var msg, JsonOptions);
        return result;
    }

    public Task<LinePayRequestResponse?> CreateAsync(LinePayRequest request, CancellationToken ct = default) =>
        RequestPaymentAsync(request, ct);

    public Task<LinePayConfirmResponse?> ConfirmAsync(LinePayConfirmInput request, CancellationToken ct = default) =>
        ConfirmPaymentAsync(request.TransactionId, request.Request, ct);

    private async Task<string?> SendAsync(string method, string apiPath, string? jsonBody,
        CancellationToken cancellationToken)
    {
        var nonce = Guid.NewGuid().ToString();
        var signature = BuildSignature(method, apiPath, jsonBody ?? string.Empty, nonce);
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
            ? "https://sandbox-api-pay.line.me"
            : _options.BaseUrl;

        var http = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), $"{baseUrl}{apiPath}");
        request.Headers.Add("X-LINE-ChannelId", _options.ChannelId);
        request.Headers.Add("X-LINE-Authorization", signature);
        request.Headers.Add("X-LINE-Authorization-Nonce", nonce);

        if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            request.Content = new StringContent(jsonBody ?? "", Encoding.UTF8, "application/json");
        }

        var response = await http.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseText))
        {
            return default;
        }

        return responseText;
    }

    private string BuildSignature(string method, string apiPath, string bodyOrQuery, string nonce)
    {
        var message = _options.ChannelSecret + apiPath + bodyOrQuery + nonce;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChannelSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    private LinePayRedirectUrls NormalizeRedirectUrls(LinePayRedirectUrls urls)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return urls;
        }

        var confirm = BuildPublicUrl(_options.PublicBaseUrl, urls.ConfirmUrl);
        var cancel = BuildPublicUrl(_options.PublicBaseUrl, urls.CancelUrl);
        return urls with { ConfirmUrl = confirm, CancelUrl = cancel };
    }

    private static string BuildPublicUrl(string publicBaseUrl, string target)
    {
        if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return target;
        }

        var baseUrl = publicBaseUrl.TrimEnd('/');
        var path = target.StartsWith('/') ? target : "/" + target;
        return baseUrl + path;
    }
}