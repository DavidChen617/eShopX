using System.Text.Json.Serialization;

namespace Infrastructure.Payments.PayPal.Models;

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

public record PayPalCaptureRequest(string OrderId);
