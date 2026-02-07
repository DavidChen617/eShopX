namespace Infrastructure.Data.Repositories;

public class EfRepository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity> where TEntity : class
{
    public EfRepository(EShopContext dbContext) : base(dbContext)
    {
    }
}
