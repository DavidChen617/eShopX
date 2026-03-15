using eShopX.Application.Exceptions;

namespace Infrastructure.Data;

public class EfUnitOfWork(EShopContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
                await entry.ReloadAsync(cancellationToken);

            throw new ConcurrencyException(ex.Entries.Select(e => e.Entity));
        }
    }
}
