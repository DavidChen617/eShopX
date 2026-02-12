using System.Net.Http.Json;
using System.Text.Json.Serialization;

using eShopX.Common.Exceptions;
using eShopX.Common.Extensions;
using Infrastructure.Options;

using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.ThirdPartyAuth;

public class LineAuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<LineAuthOptions> options,
    IRepository<User> userRepository,
    IRepository<ExternalLogin> externalLoginRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IJwtService jwtService
) : IThirdPartyAuthService<LineAuthRequest, LineAuthResponse>
{
    private readonly LineAuthOptions _options = options.Value;
    private const string ProviderName = "LINE";

    public async Task<LineAuthResponse> AuthAsync(LineAuthRequest request)
    {
        var authResponse = await ExchangeTokenAsync(request.Code, request.CodeVerifier);

        if (string.IsNullOrWhiteSpace(authResponse.IdToken))
            throw new BadRequestException("LINE Auth Error: No id_token");

        var idTokenPayload = await VerifyIdTokenAsync(authResponse.IdToken, request.Nonce);
        Console.WriteLine(idTokenPayload.ToJson());
        var sub = idTokenPayload.Sub;
        var email = idTokenPayload.Email ?? string.Empty;
        var name = idTokenPayload.Name ?? string.Empty;
        var now = DateTime.UtcNow;
        var externalLogin =
            await externalLoginRepository.FirstOrDefaultAsync(x =>
                x.LoginProvider == ProviderName && x.ProviderUserId == sub);

        User? user;

        if (externalLogin != null)
        {
            user = await userRepository.GetByIdAsync(externalLogin.UserId)
                   ?? throw new BadRequestException("LINE Auth Error: User not found");

            externalLogin.LastLoginAt = now;
            externalLoginRepository.Update(externalLogin);
            if (!string.IsNullOrWhiteSpace(idTokenPayload.Picture) && user.AvatarUrl != idTokenPayload.Picture)
            {
                user.AvatarUrl = idTokenPayload.Picture;
            }
            userRepository.Update(user);
        }
        else
        {
            user = await userRepository.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                user = new User
                {
                    Name = name,
                    Email = email,
                    Phone = string.Empty,
                    PasswordHash = string.Empty,
                    AvatarUrl = idTokenPayload.Picture
                };
                await userRepository.AddAsync(user);
            }

            externalLogin = new ExternalLogin
            {
                UserId = user.Id,
                LoginProvider = ProviderName,
                ProviderUserId = sub,
                EmailAtLinkTime = email,
                LastLoginAt = now
            };
            await externalLoginRepository.AddAsync(externalLogin);
        }

        await userRepository.SaveChangesAsync();
        await externalLoginRepository.SaveChangesAsync();

        var roles = new List<string>();
        if (user.IsAdmin) roles.Add("Admin");
        if (user.IsSeller) roles.Add("Seller");

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email, user.Name, roles);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtService.AccessTokenExpirationMinutes);

        RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpireAt = DateTime.UtcNow.AddDays(jwtService.RefreshTokenExpirationDays),
            IsRevoked = false
        };

        await refreshTokenRepository.AddAsync(refreshToken);
        await refreshTokenRepository.SaveChangesAsync();

        return new LineAuthResponse(
            accessToken, refreshToken.Token, user.Id, user.Name, accessTokenExpiresAt, sub, email);
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
        {
            form["code_verifier"] = codeVerifier;
        }

        var resp = await http.PostAsync("https://api.line.me/oauth2/v2.1/token",
            new FormUrlEncodedContent(form));

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new BadRequestException("LINE Auth Error: " + err);
        }

        return (await resp.Content.ReadFromJsonAsync<LineTokenResponse>())!;
    }

    private async Task<LineIdTokenPayload> VerifyIdTokenAsync(string idToken, string? nonce)
    {
        var http = httpClientFactory.CreateClient();
        var form = new Dictionary<string, string>
        {
            ["id_token"] = idToken,
            ["client_id"] = _options.ChannelId,
        };

        if (!string.IsNullOrWhiteSpace(nonce))
        {
            form["nonce"] = nonce;
        }

        var resp = await http.PostAsync("https://api.line.me/oauth2/v2.1/verify",
            new FormUrlEncodedContent(form));

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new BadRequestException("LINE Auth Error: " + err);
        }

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
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("id_token")]
    string? IdToken,
    [property: JsonPropertyName("refresh_token")]
    string? RefreshToken,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("token_type")]
    string TokenType
);

public record LineIdTokenPayload(
    [property: JsonPropertyName("iss")] string Iss,
    [property: JsonPropertyName("sub")] string Sub,
    [property: JsonPropertyName("aud")] string Aud,
    [property: JsonPropertyName("exp")] long Exp,
    [property: JsonPropertyName("iat")] long Iat,
    [property: JsonPropertyName("nonce")] string? Nonce,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("picture")]
    string? Picture,
    [property: JsonPropertyName("email")] string? Email
);
