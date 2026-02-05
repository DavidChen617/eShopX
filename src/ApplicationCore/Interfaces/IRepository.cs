namespace ApplicationCore.Interfaces;

public interface IRepository<T> : IReadRepository<T>
{
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}