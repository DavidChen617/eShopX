namespace eShopX.Domain.Aggregates.Users;

public sealed class User : AggregateRoot
{
    private readonly List<UserAuthProvider> _authProviders = [];

    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Role Roles { get; private set; }
    public Avatar? Avatar { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<UserAuthProvider> AuthProviders => _authProviders;

    private User() { }

    public static User Create(string name, string email)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Roles = Role.Customer,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string name)
    {
        Name = name;
    }

    public void UpdateAvatar(string url, string publicId)
    {
        Avatar = new Avatar(url, publicId);
    }

    public void AddAuthProvider(Provider provider, string? providerUserId, string? passwordHash)
    {
        var authProvider = UserAuthProvider.Create(Id, provider, providerUserId, passwordHash);
        _authProviders.Add(authProvider);
    }
}
