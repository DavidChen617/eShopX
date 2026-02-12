using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using eShopX.Common.Exceptions;
using eShopX.Common.Extensions;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.PayPal;

public class PayPalClient(HttpClient http, IOptions<PayPalOptions> options)
{
    private readonly PayPalOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    public async Task<PayPalAccessToken> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var auth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string> { ["grant_type"] = "client_credentials" });

        var resp = await http.SendAsync(request, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException($"PayPal get token failed: {json}");

        if (!json.TryParseJson<PayPalAccessToken>(out var token, out var errorMsg, JsonOptions))
            throw new ExternalServiceException($"PayPal token parse failed: {errorMsg}");

        return token!;
    }

    public async Task<PayPalCreateOrderResponse> CreateOrderAsync(
        string accessToken,
        PayPalCreateOrderRequest request,
        CancellationToken ct = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var resp = await http.SendAsync(httpRequest, ct);
        var text = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException($"PayPal create order failed: {text}");

        if (!text.TryParseJson<PayPalCreateOrderResponse>(out var response, out var errorMsg, JsonOptions))
            throw new ExternalServiceException($"PayPal create order parse failed: {errorMsg}");

        return response!;
    }

    public async Task<PayPalCaptureOrderResponse> CaptureOrderAsync(
        string accessToken,
        string orderId,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/v2/checkout/orders/{orderId}/capture");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var resp = await http.SendAsync(request, ct);
        var text = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException($"PayPal capture order failed: {text}");

        if (!text.TryParseJson<PayPalCaptureOrderResponse>(out var response, out var errorMsg, JsonOptions))
            throw new ExternalServiceException($"PayPal capture order parse failed: {errorMsg}");

        return response!;
    }
}
