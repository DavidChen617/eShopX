using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using eShopX.Common.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.PayPal;

public class PayPalService(IHttpClientFactory httpClientFactory, IOptions<PayPalOptions> options)
    : IPaymentService<PayPalCreateOrderRequest, PayPalCreateOrderResponse, PayPalCaptureRequest, PayPalCaptureOrderResponse>
{
    private readonly PayPalOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    public async Task<PayPalAccessToken> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient();
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        });

        var resp = await http.PostAsync($"{_options.BaseUrl}/v1/oauth2/token", body, ct);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);
        var token = JsonSerializer.Deserialize<PayPalAccessToken>(json, JsonOptions)!;
        Console.WriteLine($"[PayPal] BaseUrl: {_options.BaseUrl}");
        Console.WriteLine($"[PayPal] ClientId length: {_options.ClientId?.Length ?? 0}");
        Console.WriteLine($"[PayPal] TokenType: {token.TokenType}");
        Console.WriteLine($"[PayPal] AccessToken length: {token.AccessToken?.Length ?? 0}");
        return token;
    }

    public async Task<PayPalCreateOrderResponse> CreateOrderAsync(PayPalCreateOrderRequest request, CancellationToken ct =
        default)
    {
        var token = await GetAccessTokenAsync(ct);
        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var json = JsonSerializer.Serialize(request, JsonOptions);
        Console.WriteLine($"[PayPal] CreateOrder request: {json}");
        var resp = await http.PostAsync($"{_options.BaseUrl}/v2/checkout/orders",
            new StringContent(json, Encoding.UTF8, "application/json"), ct);

        var text = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine($"[PayPal] CreateOrder failed: {text}");
            throw new BadRequestException($"PayPal create order failed: {text}");
        }

        return JsonSerializer.Deserialize<PayPalCreateOrderResponse>(text, JsonOptions)!;
    }
    public async Task<PayPalCaptureOrderResponse> CaptureOrderAsync(string orderId, CancellationToken ct = default)
    {
        var token = await GetAccessTokenAsync(ct);
        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var resp = await http.PostAsync(
            $"{_options.BaseUrl}/v2/checkout/orders/{orderId}/capture",
            new StringContent("{}", Encoding.UTF8, "application/json"),
            ct);
        resp.EnsureSuccessStatusCode();
        var text = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<PayPalCaptureOrderResponse>(text, JsonOptions)!;
    }

    public Task<PayPalCreateOrderResponse> CreateAsync(PayPalCreateOrderRequest request, CancellationToken ct = default) =>
        CreateOrderAsync(request, ct);

    public Task<PayPalCaptureOrderResponse> ConfirmAsync(PayPalCaptureRequest request, CancellationToken ct = default) =>
        CaptureOrderAsync(request.OrderId, ct);
}

public record PayPalAccessToken(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);

public record PayPalCreateOrderRequest(
    [property: JsonPropertyName("intent")] string Intent,
    [property: JsonPropertyName("purchase_units")] List<PayPalPurchaseUnit> PurchaseUnits,
    [property: JsonPropertyName("application_context")] PayPalApplicationContext ApplicationContext
);

public record PayPalPurchaseUnit(
    [property: JsonPropertyName("reference_id")] string ReferenceId,
    [property: JsonPropertyName("amount")] PayPalAmount Amount
);

public record PayPalAmount(
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("value")] string Value
);

public record PayPalApplicationContext(
    [property: JsonPropertyName("return_url")] string ReturnUrl,
    [property: JsonPropertyName("cancel_url")] string CancelUrl,
    [property: JsonPropertyName("user_action")] string UserAction = "PAY_NOW"
);

public record PayPalCreateOrderResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("links")] List<PayPalLink>? Links
);

public record PayPalLink(
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("rel")] string Rel,
    [property: JsonPropertyName("method")] string Method
);

public record PayPalCaptureOrderResponse(
    string Id,
    string Status
);

public record PayPalCaptureRequest(string OrderId);
