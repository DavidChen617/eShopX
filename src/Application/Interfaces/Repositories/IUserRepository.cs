using Domain.Aggregates.Users;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> FindByProviderAsync(Provider provider, string externalId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
}
