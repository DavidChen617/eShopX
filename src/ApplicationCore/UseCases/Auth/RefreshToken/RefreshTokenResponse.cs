namespace ApplicationCore.UseCases.Auth.RefreshToken;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);