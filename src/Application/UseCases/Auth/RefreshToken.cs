using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using Domain.Aggregates.Users;

namespace Application.UseCases.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>, IValidatable<RefreshTokenCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<RefreshTokenCommand> builder)
    {
        builder.For(x => x.RefreshToken)
            .NotEmpty("Refresh token is required.");
    }
}

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

public class RefreshTokenHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IValidator validator) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result<RefreshTokenResponse>.Invalid(validation.Errors);

        var existing = await refreshTokenRepository.FindByTokenAsync(command.RefreshToken, cancellationToken);

        if (existing is null)
            return Result<RefreshTokenResponse>.NotFound(new Error("refresh_token_not_found", "Refresh token not found."));

        if (existing.IsRevoked)
            return Result<RefreshTokenResponse>.BadRequest(new Error("refresh_token_revoked", "Refresh token has been revoked."));

        if (existing.ExpireAt < DateTime.UtcNow)
            return Result<RefreshTokenResponse>.BadRequest(new Error("refresh_token_expired", "Refresh token has expired."));

        var user = await userRepository.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null)
            return Result<RefreshTokenResponse>.NotFound(new Error("user_not_found", "User not found."));

        existing.Revoke();
        refreshTokenRepository.Update(existing);

        var roles = Enum.GetValues<Role>().Where(r => user.Roles.HasFlag(r)).Select(r => r.ToString());
        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Email, user.Name, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(tokenGenerator.AccessTokenExpirationMinutes);

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            tokenGenerator.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(tokenGenerator.RefreshTokenExpirationDays));

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenResponse>.Ok(new RefreshTokenResponse(
            accessToken,
            newRefreshToken.Token,
            expiresAt));
    }
}
