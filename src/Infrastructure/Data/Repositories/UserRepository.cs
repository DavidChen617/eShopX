using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Users;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class UserRepository(EShopContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        => db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> FindByProviderAsync(Provider provider, string externalId, CancellationToken cancellationToken = default)
        => db.UserAuthProviders
            .Where(p => p.Provider == provider && p.ProviderUserId == externalId)
            .Join(db.Users, p => p.UserId, u => u.Id, (_, u) => u)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await db.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => db.Users.Update(user);
}
