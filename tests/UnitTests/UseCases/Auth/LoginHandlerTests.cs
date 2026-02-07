using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Auth.Login;

using eShopX.Common.Exceptions;

using Moq;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Auth;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsValidationException()
    {
        var userRepo = new InMemoryRepository<User>();
        var refreshRepo = new InMemoryRepository<RefreshToken>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var jwtService = new Mock<IJwtService>();

        var handler = new LoginHandler(userRepo, refreshRepo, passwordHasher.Object, jwtService.Object);

        var command = new LoginCommand("missing@example.com", "password");

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsValidationException()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };
        var userRepo = new InMemoryRepository<User>([user]);
        var refreshRepo = new InMemoryRepository<RefreshToken>();
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(p => p.VerifyPassword("hash", "bad")).Returns(false);
        var jwtService = new Mock<IJwtService>();

        var handler = new LoginHandler(userRepo, refreshRepo, passwordHasher.Object, jwtService.Object);

        var command = new LoginCommand("user@example.com", "bad");

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndPersistsRefreshToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Name = "Test", PasswordHash = "hash" };
        var userRepo = new InMemoryRepository<User>([user]);
        var refreshRepo = new InMemoryRepository<RefreshToken>();
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(p => p.VerifyPassword("hash", "good")).Returns(true);

        var jwtService = new Mock<IJwtService>();
        jwtService.SetupGet(j => j.AccessTokenExpirationMinutes).Returns(30);
        jwtService.SetupGet(j => j.RefreshTokenExpirationDays).Returns(7);
        jwtService.Setup(j => j.GenerateAccessToken(user.Id, user.Email, user.Name, It.IsAny<IEnumerable<string>>()))
            .Returns("access-token");
        jwtService.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");

        var handler = new LoginHandler(userRepo, refreshRepo, passwordHasher.Object, jwtService.Object);

        var response = await handler.Handle(new LoginCommand("user@example.com", "good"), CancellationToken.None);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal(user.Id, response.UserId);
        Assert.Single(refreshRepo.Data);
        Assert.Equal("refresh-token", refreshRepo.Data[0].Token);
        Assert.False(refreshRepo.Data[0].IsRevoked);
    }
}
