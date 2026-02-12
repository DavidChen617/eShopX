using System.Text.Json.Serialization;

namespace Infrastructure.Payments.PayPal;

public class PayPalService(PayPalClient client) :
    ICreatePaymentService<PayPalCreateOrderRequest, PayPalCreateOrderResponse>,
    IConfirmPaymentService<PayPalCaptureRequest, PayPalCaptureOrderResponse>
{
    public async Task<PayPalCreateOrderResponse> CreateAsync(
        PayPalCreateOrderRequest request, CancellationToken ct = default)
    {
        var token = await client.GetAccessTokenAsync(ct);
        return await client.CreateOrderAsync(token.AccessToken, request, ct);
    }

    public async Task<PayPalCaptureOrderResponse> ConfirmAsync(
        PayPalCaptureRequest request, CancellationToken ct = default)
    {
        var token = await client.GetAccessTokenAsync(ct);
        return await client.CaptureOrderAsync(token.AccessToken, request.OrderId, ct);
    }
}

public record PayPalAccessToken(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("token_type")]
    string TokenType,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn
);

public record PayPalCreateOrderRequest(
    [property: JsonPropertyName("intent")] string Intent,
    [property: JsonPropertyName("purchase_units")]
    List<PayPalPurchaseUnit> PurchaseUnits,
    [property: JsonPropertyName("application_context")]
    PayPalApplicationContext ApplicationContext
);

public record PayPalPurchaseUnit(
    [property: JsonPropertyName("reference_id")]
    string ReferenceId,
    [property: JsonPropertyName("amount")] PayPalAmount Amount
);

public record PayPalAmount(
    [property: JsonPropertyName("currency_code")]
    string CurrencyCode,
    [property: JsonPropertyName("value")] string Value
);

public record PayPalApplicationContext(
    [property: JsonPropertyName("return_url")]
    string ReturnUrl,
    [property: JsonPropertyName("cancel_url")]
    string CancelUrl,
    [property: JsonPropertyName("user_action")]
    string UserAction = "PAY_NOW"
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
