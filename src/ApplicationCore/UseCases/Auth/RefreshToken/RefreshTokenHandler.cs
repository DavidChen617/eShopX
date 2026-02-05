namespace ApplicationCore.UseCases.Auth.RefreshToken;

public class RefreshTokenHandler(
    IRepository<Entities.RefreshToken> refreshTokenRepository,
    IRepository<User> userRepository,
    IJwtService jwtService) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await refreshTokenRepository.FirstOrDefaultAsync(
            t => t.Token == command.RefreshToken, cancellationToken);

        if (existingToken is null)
        {
            throw new ValidationException("refreshToken", "Invalid refresh token");
        }

        if (existingToken.IsRevoked)
        {
            throw new ValidationException("refreshToken", "Refresh token has been revoked");
        }

        if (existingToken.ExpireAt < DateTime.UtcNow)
        {
            throw new ValidationException("refreshToken", "Refresh token has expired");
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", existingToken.UserId);
        }

        existingToken.IsRevoked = true;
        refreshTokenRepository.Update(existingToken);

        var roles = new List<string>();
        if (user.IsAdmin) roles.Add("Admin");
        if (user.IsSeller) roles.Add("Seller");

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email, user.Name, roles);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtService.AccessTokenExpirationMinutes);

        Entities.RefreshToken newRefreshToken = new()
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpireAt = DateTime.UtcNow.AddDays(jwtService.RefreshTokenExpirationDays),
            IsRevoked = false
        };

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(
            accessToken,
            newRefreshToken.Token,
            accessTokenExpiresAt);
    }
}