using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Auth.RefreshToken;

using eShopX.Common.Exceptions;

using Moq;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Auth;

public class RefreshTokenHandlerTests
{
    [Fact]
    public async Task Handle_TokenNotFound_ThrowsValidationException()
    {
        var tokenRepo = new InMemoryRepository<RefreshToken>();
        var userRepo = new InMemoryRepository<User>();
        var jwtService = new Mock<IJwtService>();

        var handler = new RefreshTokenHandler(tokenRepo, userRepo, jwtService.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new RefreshTokenCommand("missing"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsValidationException()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "revoked",
            ExpireAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };
        var tokenRepo = new InMemoryRepository<RefreshToken>([token]);
        var userRepo = new InMemoryRepository<User>();
        var jwtService = new Mock<IJwtService>();

        var handler = new RefreshTokenHandler(tokenRepo, userRepo, jwtService.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new RefreshTokenCommand("revoked"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsValidationException()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "expired",
            ExpireAt = DateTime.UtcNow.AddMinutes(-1),
            IsRevoked = false
        };
        var tokenRepo = new InMemoryRepository<RefreshToken>([token]);
        var userRepo = new InMemoryRepository<User>();
        var jwtService = new Mock<IJwtService>();

        var handler = new RefreshTokenHandler(tokenRepo, userRepo, jwtService.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new RefreshTokenCommand("expired"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserMissing_ThrowsNotFoundException()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "valid",
            ExpireAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };
        var tokenRepo = new InMemoryRepository<RefreshToken>([token]);
        var userRepo = new InMemoryRepository<User>();
        var jwtService = new Mock<IJwtService>();

        var handler = new RefreshTokenHandler(tokenRepo, userRepo, jwtService.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RefreshTokenCommand("valid"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesOldTokenAndReturnsNewTokens()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Name = "Test" };
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "valid",
            ExpireAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        var tokenRepo = new InMemoryRepository<RefreshToken>([token]);
        var userRepo = new InMemoryRepository<User>([user]);

        var jwtService = new Mock<IJwtService>();
        jwtService.SetupGet(j => j.AccessTokenExpirationMinutes).Returns(15);
        jwtService.SetupGet(j => j.RefreshTokenExpirationDays).Returns(7);
        jwtService.Setup(j => j.GenerateAccessToken(user.Id, user.Email, user.Name, It.IsAny<IEnumerable<string>>()))
            .Returns("access");
        jwtService.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh");

        var handler = new RefreshTokenHandler(tokenRepo, userRepo, jwtService.Object);

        var response = await handler.Handle(new RefreshTokenCommand("valid"), CancellationToken.None);

        Assert.Equal("access", response.AccessToken);
        Assert.Equal("new-refresh", response.RefreshToken);
        Assert.True(token.IsRevoked);
        Assert.Equal(2, tokenRepo.Data.Count);
    }
}