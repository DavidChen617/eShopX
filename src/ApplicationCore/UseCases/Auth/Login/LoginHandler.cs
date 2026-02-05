namespace ApplicationCore.UseCases.Auth.Login;

public class LoginHandler(
    IRepository<User> userRepository,
    IRepository<Entities.RefreshToken> refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FirstOrDefaultAsync(
            u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            throw new ValidationException("credentials", "Email or password is incorrect");
        }

        var isValid = passwordHasher.VerifyPassword(user.PasswordHash, command.Password);

        if (!isValid)
        {
            throw new ValidationException("credentials", "Email or password is incorrect");
        }

        var roles = new List<string>();
        if (user.IsAdmin) roles.Add("Admin");
        if (user.IsSeller) roles.Add("Seller");

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email, user.Name, roles);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtService.AccessTokenExpirationMinutes);

        Entities.RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpireAt = DateTime.UtcNow.AddDays(jwtService.RefreshTokenExpirationDays),
            IsRevoked = false
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshToken.Token,
            user.Id,
            user.Name,
            accessTokenExpiresAt);
    }
}