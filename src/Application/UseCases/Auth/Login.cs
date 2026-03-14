using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;

namespace eShopX.Application.UseCases.Auth;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<LoginResponse>>, IValidatable<LoginCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<LoginCommand> builder)
    {
        builder.For(x => x.Email)
            .NotEmpty("Email is required.")
            .EmailAddress("Email format is invalid.");

        builder.For(x => x.Password)
            .NotEmpty("Password is required.");
    }
}

public record LoginTokenData(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Name,
    DateTime ExpiresAt) : IMapFrom<User, LoginTokenData, LoginResponse>
{
    public LoginResponse MapFrom(User user, LoginTokenData token) =>
        new(token.AccessToken, token.RefreshToken, user.Id, user.Name, token.ExpiresAt);
}

public class LoginHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IValidator validator,
    IMapper mapper) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result<LoginResponse>.Invalid(validation.Errors);

        var user = await userRepository.FindByEmailAsync(command.Email, cancellationToken);
        var localProvider = user?.AuthProviders.FirstOrDefault(p => p.Provider == Provider.Local);

        if (localProvider?.PasswordHash is null ||
            !passwordHasher.VerifyPassword(localProvider.PasswordHash, command.Password))
        {
            return Result<LoginResponse>.Invalid(new Dictionary<string, IReadOnlyList<string>>
            {
                { "credentials", ["Email or password is incorrect."] }
            });
        }

        var roles = Enum.GetValues<Role>().Where(r => user!.Roles.HasFlag(r)).Select(r => r.ToString());
        var accessToken = tokenGenerator.GenerateAccessToken(user!.Id, user.Email, user.Name, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(tokenGenerator.AccessTokenExpirationMinutes);

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenGenerator.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(tokenGenerator.RefreshTokenExpirationDays));

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tokenData = new LoginTokenData(accessToken, refreshToken.Token, expiresAt);
        return Result<LoginResponse>.Ok(mapper.Map<User, LoginTokenData, LoginResponse>(user, tokenData));
    }
}
