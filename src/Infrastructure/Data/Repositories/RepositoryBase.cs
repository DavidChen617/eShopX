using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories;

public class RepositoryBase<TEntity>(DbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    public virtual async Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public Task<List<TResult>> QueryAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryBuilder,
        CancellationToken cancellationToken = default)
    {
        var query = queryBuilder(dbContext.Set<TEntity>());
        return query.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull
    {
        return await dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().SingleOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().AnyAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, ct);
        return entity;
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        return dbContext.Set<TEntity>().AddRangeAsync(entities, ct);
    }

    public void Remove(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        dbContext.Set<TEntity>().RemoveRange(entities);
    }

    public void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        dbContext.Set<TEntity>().UpdateRange(entities);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}