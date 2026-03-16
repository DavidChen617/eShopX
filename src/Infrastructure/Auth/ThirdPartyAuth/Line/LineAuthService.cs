using eShopX.Application.Exceptions;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;
using Infrastructure.Auth.ThirdPartyAuth.Line.Models;

namespace Infrastructure.Auth.ThirdPartyAuth.Line;

public class LineAuthService(
    LineAuthClient lineAuthClient,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : IThirdPartyAuthService<LineAuthRequest, LineAuthResponse>
{

    public async Task<LineAuthResponse> AuthAsync(LineAuthRequest request, CancellationToken cancellationToken = default)
    {
        var tokenResponse = await lineAuthClient.ExchangeTokenAsync(request.Code, request.CodeVerifier);
        if (string.IsNullOrWhiteSpace(tokenResponse.IdToken))
            throw new ExternalServiceException("LINE", "No id_token in response");

        var payload = await lineAuthClient.VerifyIdTokenAsync(tokenResponse.IdToken, request.Nonce);

        var sub = payload.Sub;
        var email = payload.Email ?? string.Empty;
        var name = payload.Name ?? sub;

        var user = await userRepository.FindByProviderAsync(Provider.Line, sub);
        if (user is null)
        {
            user = await userRepository.FindByEmailAsync(email);
            if (user is null)
            {
                user = User.Create(name, email);
                await userRepository.AddAsync(user);
            }
            user.AddAuthProvider(Provider.Line, sub, null);
        }

        if (!string.IsNullOrWhiteSpace(payload.Picture))
            user.UpdateAvatar(payload.Picture, sub);

        var roles = Enum.GetValues<Role>().Where(r => user.Roles.HasFlag(r)).Select(r => r.ToString());
        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Email, user.Name, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(tokenGenerator.AccessTokenExpirationMinutes);

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenGenerator.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(tokenGenerator.RefreshTokenExpirationDays));

        await refreshTokenRepository.AddAsync(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LineAuthResponse(accessToken, refreshToken.Token, user.Id, user.Name, expiresAt, sub, email, payload.Picture);
    }
}
