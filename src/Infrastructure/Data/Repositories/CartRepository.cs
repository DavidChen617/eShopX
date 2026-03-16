using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Carts;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class CartRepository(EShopContext db) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        => await db.Carts.AddAsync(cart, cancellationToken);

    public void Update(Cart cart)
    {
        foreach (var item in cart.Items.Where(i => db.Entry(i).State == EntityState.Detached))
            db.CartItems.Add(item);
    }
}
