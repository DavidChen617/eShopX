using System.Net.Http.Json;
using System.Text.Json.Serialization;
using eShopX.Application.Exceptions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;
using Google.Apis.Auth;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.ThirdPartyAuth;

public class GoogleAuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<GoogleAuthOptions> options,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse>
{
    private readonly GoogleAuthOptions _options = options.Value;

    public async Task<GoogleAuthResponse> AuthAsync(GoogleAuthRequest request)
    {
        var http = httpClientFactory.CreateClient();
        var form = new Dictionary<string, string>
        {
            ["code"] = request.Code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUrl,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = request.CodeVerifier
        };

        var tokenResponse = await http.PostAsync("https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(form));

        if (!tokenResponse.IsSuccessStatusCode)
            throw new ExternalServiceException("Google", await tokenResponse.Content.ReadAsStringAsync());

        var token = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        if (token?.IdToken is null)
            throw new ExternalServiceException("Google", "No id_token in response");

        var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [_options.ClientId] };
        var payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken, settings);

        if (payload.Issuer != "accounts.google.com" && payload.Issuer != "https://accounts.google.com")
            throw new ExternalServiceException("Google", $"Invalid issuer: {payload.Issuer}");

        var googleSub = payload.Subject;
        var email = payload.Email ?? throw new ExternalServiceException("Google", "Email missing from token");
        var name = payload.Name ?? email;

        var user = await userRepository.FindByProviderAsync(Provider.Google, googleSub);
        if (user is null)
        {
            user = await userRepository.FindByEmailAsync(email);
            if (user is null)
            {
                user = User.Create(name, email, null);
                await userRepository.AddAsync(user);
            }
            user.AddAuthProvider(Provider.Google, googleSub, null);
        }

        if (!string.IsNullOrWhiteSpace(payload.Picture))
            user.UpdateAvatar(payload.Picture, googleSub);

        userRepository.Update(user);

        var roles = Enum.GetValues<Role>().Where(r => user.Roles.HasFlag(r)).Select(r => r.ToString());
        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Email, user.Name, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(tokenGenerator.AccessTokenExpirationMinutes);

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenGenerator.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(tokenGenerator.RefreshTokenExpirationDays));

        await refreshTokenRepository.AddAsync(refreshToken);
        await unitOfWork.SaveChangesAsync();

        return new GoogleAuthResponse(accessToken, refreshToken.Token, user.Id, user.Name, expiresAt, googleSub, email, payload.Picture);
    }
}

public record GoogleTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("id_token")] string IdToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken
);

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
