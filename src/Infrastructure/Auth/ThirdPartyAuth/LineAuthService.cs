using System.Net.Http.Json;
using System.Text.Json.Serialization;
using eShopX.Application.Exceptions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.ThirdPartyAuth;

public class LineAuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<LineAuthOptions> options,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : IThirdPartyAuthService<LineAuthRequest, LineAuthResponse>
{
    private readonly LineAuthOptions _options = options.Value;

    public async Task<LineAuthResponse> AuthAsync(LineAuthRequest request)
    {
        var tokenResponse = await ExchangeTokenAsync(request.Code, request.CodeVerifier);
        if (string.IsNullOrWhiteSpace(tokenResponse.IdToken))
            throw new ExternalServiceException("LINE", "No id_token in response");

        var payload = await VerifyIdTokenAsync(tokenResponse.IdToken, request.Nonce);

        var sub = payload.Sub;
        var email = payload.Email ?? string.Empty;
        var name = payload.Name ?? sub;

        var user = await userRepository.FindByProviderAsync(Provider.Line, sub);
        if (user is null)
        {
            user = await userRepository.FindByEmailAsync(email);
            if (user is null)
            {
                user = User.Create(name, email, null);
                await userRepository.AddAsync(user);
            }
            user.AddAuthProvider(Provider.Line, sub, null);
        }

        if (!string.IsNullOrWhiteSpace(payload.Picture))
            user.UpdateAvatar(payload.Picture, sub);

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

        return new LineAuthResponse(accessToken, refreshToken.Token, user.Id, user.Name, expiresAt, sub, email);
    }

    private async Task<LineTokenResponse> ExchangeTokenAsync(string code, string? codeVerifier)
    {
        var http = httpClientFactory.CreateClient();
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["client_id"] = _options.ChannelId,
            ["client_secret"] = _options.ChannelSecret
        };

        if (!string.IsNullOrWhiteSpace(codeVerifier))
            form["code_verifier"] = codeVerifier;

        var resp = await http.PostAsync("https://api.line.me/oauth2/v2.1/token",
            new FormUrlEncodedContent(form));

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException("LINE", await resp.Content.ReadAsStringAsync());

        return (await resp.Content.ReadFromJsonAsync<LineTokenResponse>())!;
    }

    private async Task<LineIdTokenPayload> VerifyIdTokenAsync(string idToken, string? nonce)
    {
        var http = httpClientFactory.CreateClient();
        var form = new Dictionary<string, string>
        {
            ["id_token"] = idToken,
            ["client_id"] = _options.ChannelId
        };

        if (!string.IsNullOrWhiteSpace(nonce))
            form["nonce"] = nonce;

        var resp = await http.PostAsync("https://api.line.me/oauth2/v2.1/verify",
            new FormUrlEncodedContent(form));

        if (!resp.IsSuccessStatusCode)
            throw new ExternalServiceException("LINE", await resp.Content.ReadAsStringAsync());

        return (await resp.Content.ReadFromJsonAsync<LineIdTokenPayload>())!;
    }
}

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
