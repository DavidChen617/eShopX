using System.Linq.Expressions;

using ApplicationCore.Interfaces;

namespace UnitTests.Helpers;

public sealed class InMemoryRepository<T> : IRepository<T> where T : class
{
    public List<T> Data { get; }

    public InMemoryRepository(IEnumerable<T>? seed = null)
    {
        Data = seed?.ToList() ?? new List<T>();
    }

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Data.ToList());

    public Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Data.Where(predicate.Compile()).ToList());

    public Task<List<TResult>> QueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
        CancellationToken cancellationToken = default)
        => Task.FromResult(queryBuilder(Data.AsQueryable()).ToList());

    public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        if (id is Guid guid)
        {
            var item = Data.FirstOrDefault(entity => TryGetGuidId(entity, out var value) && value == guid);
            return Task.FromResult(item);
        }

        return Task.FromResult<T?>(null);
    }

    public Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Data.FirstOrDefault());

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Data.FirstOrDefault(predicate.Compile()));

    public Task<T?> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Data.SingleOrDefault());

    public Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Data.SingleOrDefault(predicate.Compile()));

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Data.Any());

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Data.Any(predicate.Compile()));

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Data.Count);

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Data.Count(predicate.Compile()));

    public Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        EnsureGuidId(entity);
        Data.Add(entity);
        return Task.FromResult(entity);
    }

    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            EnsureGuidId(entity);
            Data.Add(entity);
        }

        return Task.CompletedTask;
    }

    public void Remove(T entity)
        => Data.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Data.Remove(entity);
        }
    }

    public void Update(T entity)
    {
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);

    private static void EnsureGuidId(T entity)
    {
        if (!TryGetGuidId(entity, out var value))
        {
            return;
        }

        if (value == Guid.Empty)
        {
            var idProperty = typeof(T).GetProperty("Id");
            idProperty?.SetValue(entity, Guid.NewGuid());
        }
    }

    private static bool TryGetGuidId(T entity, out Guid value)
    {
        value = Guid.Empty;
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null || idProperty.PropertyType != typeof(Guid))
        {
            return false;
        }

        var raw = idProperty.GetValue(entity);
        if (raw is Guid guidValue)
        {
            value = guidValue;
            return true;
        }

        return false;
    }
}
