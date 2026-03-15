using eShopX.Application.Exceptions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;
using Google.Apis.Auth;
using Infrastructure.Auth.ThirdPartyAuth.Google.Models;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.ThirdPartyAuth.Google;

public class GoogleAuthService(
    GoogleAuthClient googleAuthClient,
    IOptions<GoogleAuthOptions> options,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse>
{
    private readonly GoogleAuthOptions _options = options.Value;

    public async Task<GoogleAuthResponse> AuthAsync(GoogleAuthRequest request)
    {
        var token = await googleAuthClient.ExchangeTokenAsync(request.Code, request.CodeVerifier);
        if (token.IdToken is null)
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
                user = User.Create(name, email);
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
