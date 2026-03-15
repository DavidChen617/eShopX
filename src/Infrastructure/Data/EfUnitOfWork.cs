using eShopX.Application.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Data;

public class EfUnitOfWork(EShopContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
