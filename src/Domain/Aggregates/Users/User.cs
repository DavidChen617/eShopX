namespace eShopX.Domain.Aggregates.Users;

public sealed class User : AggregateRoot
{
    private readonly List<UserAuthProvider> _authProviders = [];

    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public Role Roles { get; private set; }
    public Address? Address { get; private set; }
    public Avatar? Avatar { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<UserAuthProvider> AuthProviders => _authProviders;

    private User() { }

    public static User Create(string name, string email, string? phone)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Phone = phone,
            Roles = Role.Customer,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string name, string? phone)
    {
        Name = name;
        Phone = phone;
    }

    public void UpdateAvatar(string url, string publicId)
    {
        Avatar = new Avatar(url, publicId);
    }

    public void UpdateAddress(string city, string district, string street, string postalCode)
    {
        Address = new Address(city, district, street, postalCode);
    }

    public void AddAuthProvider(Provider provider, string? providerUserId, string? passwordHash)
    {
        var authProvider = UserAuthProvider.Create(Id, provider, providerUserId, passwordHash);
        _authProviders.Add(authProvider);
    }
}
