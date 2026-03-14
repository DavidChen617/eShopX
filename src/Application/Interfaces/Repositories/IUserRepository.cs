using eShopX.Domain.Aggregates.Users;

namespace eShopX.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
