namespace eShopX.Application.Exceptions;

public class ConcurrencyException(IEnumerable<object> staleEntities)
    : Exception("Concurrency conflict detected.")
{
    public IReadOnlyList<object> StaleEntities { get; } = staleEntities.ToList();
}
