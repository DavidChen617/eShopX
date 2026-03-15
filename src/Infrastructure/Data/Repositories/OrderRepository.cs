using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Orders;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class OrderRepository(EShopContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        Guid? userId,
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = db.Orders.AsQueryable();

        if (userId.HasValue) query = query.Where(o => o.UserId == userId.Value);
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await db.Orders.AddAsync(order, cancellationToken);

    public void Update(Order order)
        => db.Orders.Update(order);
}
