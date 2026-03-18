using Domain.Aggregates.Orders;

namespace Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        Guid? userId,
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
    void Delete(Order order);
}
