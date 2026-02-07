namespace ApplicationCore.UseCases.Auth.Logout;

public class LogoutHandler(IRepository<Entities.RefreshToken> refreshTokenRepository)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var refreshToken = await refreshTokenRepository.FirstOrDefaultAsync(
            rt => rt.Token == command.RefreshToken && !rt.IsRevoked,
            cancellationToken);

        if (refreshToken is null)
        {
            return; // Token is missing or already revoked; no-op
        }

        refreshToken.IsRevoked = true;
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
