using Domain.Aggregates.Carts;

namespace Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
    void Update(Cart cart);
}
