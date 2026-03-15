using System.Text.Json.Serialization;

namespace Infrastructure.Payments.PayPal.Models;

public record PayPalAccessToken(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
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
