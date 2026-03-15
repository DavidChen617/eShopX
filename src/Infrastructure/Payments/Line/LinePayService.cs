using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using eShopX.Application.Exceptions;
using Infrastructure.Options;
using Infrastructure.Payments.Line.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.Line;

public class LinePayService(IHttpClientFactory httpClientFactory, IOptions<LinePayOptions> options)
{
    private readonly LinePayOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<LinePayRequestResponse> RequestPaymentAsync(LinePayRequest request,
        CancellationToken ct = default)
    {
        var path = "/v3/payments/request";
        var normalizedRequest = request with { RedirectUrls = NormalizeRedirectUrls(request.RedirectUrls) };
        var body = JsonSerializer.Serialize(normalizedRequest, JsonOptions);
        var text = await SendAsync("POST", path, body, ct);

        return JsonSerializer.Deserialize<LinePayRequestResponse>(text, JsonOptions)
               ?? throw new ExternalServiceException("LinePay", "Empty response from payment request.");
    }

    public async Task<LinePayConfirmResponse> ConfirmPaymentAsync(long transactionId, LinePayConfirmRequest request,
        CancellationToken ct = default)
    {
        var path = $"/v3/payments/{transactionId}/confirm";
        var body = JsonSerializer.Serialize(request, JsonOptions);
        var text = await SendAsync("POST", path, body, ct);

        return JsonSerializer.Deserialize<LinePayConfirmResponse>(text, JsonOptions)
               ?? throw new ExternalServiceException("LinePay", "Empty response from confirm.");
    }

    private async Task<string> SendAsync(string method, string apiPath, string? jsonBody,
        CancellationToken cancellationToken)
    {
        var nonce = Guid.NewGuid().ToString();
        var signature = BuildSignature(method, apiPath, jsonBody ?? string.Empty, nonce);
        var http = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), $"{_options.BaseUrl}{apiPath}");
        request.Headers.Add("X-LINE-ChannelId", _options.ChannelId);
        request.Headers.Add("X-LINE-Authorization", signature);
        request.Headers.Add("X-LINE-Authorization-Nonce", nonce);

        if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            request.Content = new StringContent(jsonBody ?? "", Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        var response = await http.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseText))
            throw new ExternalServiceException("LinePay", "API returned empty response.");

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
