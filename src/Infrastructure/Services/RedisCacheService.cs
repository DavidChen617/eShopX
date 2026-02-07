using eShopX.Common.Extensions;

using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer multiplexer) : ICacheService
{
    private readonly IDatabase _db = multiplexer.GetDatabase();
    private const string KeyPrefix = "eshopx:";

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = KeyPrefix + key;
        var cached = await _db.StringGetAsync(cacheKey);
        if (cached.HasValue && !cached.IsNullOrEmpty)
        {
            if (cached.ToString().TryParseJson<T>(out var res, out _, null) && res is not null)
            {
                return res;
            }

            await _db.KeyDeleteAsync(cacheKey);
        }

        var value = await factory(cancellationToken);
        var payload = value.ToJson();
        await _db.StringSetAsync(cacheKey, payload, ttl);
        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _db.KeyDeleteAsync(KeyPrefix + key);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var endpoints = multiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = multiplexer.GetServer(endpoint);
            if (!server.IsConnected)
            {
                continue;
            }

            var pattern = KeyPrefix + prefix + "*";
            foreach (var key in server.Keys(database: _db.Database, pattern: pattern))
            {
                await _db.KeyDeleteAsync(key);
            }
        }
    }
}
