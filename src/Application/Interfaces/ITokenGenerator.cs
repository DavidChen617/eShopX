namespace eShopX.Application.Interfaces;

public interface ITokenGenerator
{
    int AccessTokenExpirationMinutes { get; }
    int RefreshTokenExpirationDays { get; }
    string GenerateAccessToken(Guid userId, string email, string name, IEnumerable<string>? roles = null);
    string GenerateRefreshToken();
}
