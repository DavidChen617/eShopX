using System.Linq.Expressions;
using eShopX.Common.Exceptions;

namespace Infrastructure.Data.Repositories;

public class RepositoryBase<TEntity>(DbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    private IQueryable<TEntity> ReadQuery => dbContext.Set<TEntity>().AsNoTracking();

    public virtual async Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await ReadQuery.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ReadQuery.Where(predicate).ToListAsync(cancellationToken);
    }

    public Task<List<TResult>> QueryAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> queryBuilder,
        CancellationToken cancellationToken = default)
    {
        var query = queryBuilder(ReadQuery);
        return query.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull
    {
        return await ReadQuery.FirstOrDefaultAsync(
            e => EF.Property<TId>(e, "Id")!.Equals(id),
            cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await ReadQuery.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ReadQuery.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await ReadQuery.SingleOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ReadQuery.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await ReadQuery.AnyAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ReadQuery.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await ReadQuery.CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ReadQuery.CountAsync(predicate, cancellationToken);
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

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrencyException("The information has been modified by someone else, please try again. " + e.Message); 
        }
    }
}
