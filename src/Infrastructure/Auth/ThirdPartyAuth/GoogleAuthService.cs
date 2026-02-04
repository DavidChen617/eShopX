using System.Net.Http.Json;
using System.Text.Json.Serialization;
using eShopX.Common.Exceptions;
using Google.Apis.Auth;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.ThirdPartyAuth;

public class GoogleAuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<GoogleAuthOptions> options,
    IRepository<User> userRepository,
    IRepository<ExternalLogin> externalLoginRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IJwtService jwtService) : IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse>
{
    private readonly GoogleAuthOptions _options = options.Value;
    private const string ProviderName = "Google";

    public async Task<GoogleAuthResponse> AuthAsync(GoogleAuthRequest request)
    {
        var tokenEndpoint = "https://oauth2.googleapis.com/token";
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
        var tokenResponse = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var err = await tokenResponse.Content.ReadAsStringAsync();
            throw new BadRequestException("Google Auth Error: " + err);
        }

        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

        if (token?.IdToken is null)
            throw new BadRequestException("Google Auth Error: No Token Found");

        var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [_options.ClientId] };
        var payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken, settings);

        // Check the issuer additionally (Google official recommendation)
        if (payload.Issuer != "accounts.google.com" && payload.Issuer != "https://accounts.google.com")
            throw new BadRequestException($"Google Auth Error: Invalid ISSUER : {payload.Issuer}");

        var googleSub = payload.Subject;
        var email = payload.Email;
        var name = payload.Name;
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Google Auth Error: Email missing");
        }

        var externalLogin = await externalLoginRepository.FirstOrDefaultAsync(
            x => x.LoginProvider == ProviderName && x.ProviderUserId == googleSub);

        User? user;
        if (externalLogin is not null)
        {
            user = await userRepository.GetByIdAsync(externalLogin.UserId)
                   ?? throw new BadRequestException("Google Auth Error: User not found");

            externalLogin.LastLoginAt = now;
            if (!string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(externalLogin.EmailAtLinkTime))
            {
                externalLogin.EmailAtLinkTime = email;
            }
            externalLoginRepository.Update(externalLogin);

            if (!string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(user.Email))
            {
                user.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(user.Name))
            {
                user.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(payload.Picture) && user.AvatarUrl != payload.Picture)
            {
                user.AvatarUrl = payload.Picture;
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
                    Name = name ?? string.Empty,
                    Email = email,
                    Phone = string.Empty,
                    PasswordHash = string.Empty,
                    AvatarUrl = payload.Picture
                };

                await userRepository.AddAsync(user);
            }

            user = user ?? throw new BadRequestException("Google Auth Error: User missing");
            externalLogin = new ExternalLogin
            {
                UserId = user.Id,
                LoginProvider = ProviderName,
                ProviderUserId = googleSub,
                EmailAtLinkTime = email,
                LastLoginAt = now
            };

            await externalLoginRepository.AddAsync(externalLogin);
        }

        await userRepository.SaveChangesAsync();

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

        // 4) 回你自己的登入結果（例如 JWT）
        return new GoogleAuthResponse(
            accessToken,
            refreshToken.Token,
            user.Id,
            user.Name,
            accessTokenExpiresAt,
            googleSub,
            user.Email,
            payload.Picture
            );
    }
}

public record TokenResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("id_token")]
    string IdToken,
    [property: JsonPropertyName("token_type")]
    string TokenType,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,
    [property: JsonPropertyName("refresh_token")]
    string? RefreshToken
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
