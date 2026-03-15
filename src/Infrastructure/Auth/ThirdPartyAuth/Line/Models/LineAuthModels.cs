using System.Text.Json.Serialization;

namespace Infrastructure.Auth.ThirdPartyAuth.Line.Models;

public record LineAuthRequest(string Code, string? CodeVerifier, string? Nonce);

public record LineAuthResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Name,
    DateTime ExpiresAt,
    string LineSub,
    string Email
);

public record LineTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("id_token")] string? IdToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("token_type")] string TokenType
);

public record LineIdTokenPayload(
    [property: JsonPropertyName("iss")] string Iss,
    [property: JsonPropertyName("sub")] string Sub,
    [property: JsonPropertyName("aud")] string Aud,
    [property: JsonPropertyName("exp")] long Exp,
    [property: JsonPropertyName("iat")] long Iat,
    [property: JsonPropertyName("nonce")] string? Nonce,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("picture")] string? Picture,
    [property: JsonPropertyName("email")] string? Email
);
