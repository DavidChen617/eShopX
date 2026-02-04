namespace ApplicationCore.UseCases.Auth.Login;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Name,
    DateTime ExpiresAt);
