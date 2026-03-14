namespace eShopX.Domain.Aggregates.Users;

public sealed class UserAuthProvider : Entity
{
    public Guid UserId { get; private set; }
    public Provider Provider { get; private set; }
    public string? ProviderUserId { get; private set; }
    public string? PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserAuthProvider() { }

    public static UserAuthProvider Create(Guid userId, Provider provider, string? providerUserId, string? passwordHash)
    {
        return new UserAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }
}
