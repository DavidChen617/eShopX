using System.Text.Json.Serialization;

namespace Infrastructure.Auth.ThirdPartyAuth.Google.Models;

public record GoogleAuthRequest(string Code, string CodeVerifier, string State);

public record GoogleAuthResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Name,
    DateTime ExpiresAt,
    string GoogleSub,
    string Email,
    string? Picture
);

public record GoogleTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("id_token")] string IdToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken
);
