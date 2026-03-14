using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;

namespace eShopX.Application.UseCases.Auth;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;

public class LogoutHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await refreshTokenRepository.FindByTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken is null || refreshToken.IsRevoked)
            return Result.NoContent();

        refreshToken.Revoke();
        refreshTokenRepository.Update(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
