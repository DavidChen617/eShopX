using eShopX.Common.Proxy;

namespace ApplicationCore.Interfaces;

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface ICacheService : IInterceptable
{
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default) where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}