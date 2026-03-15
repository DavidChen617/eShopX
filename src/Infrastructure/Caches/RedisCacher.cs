using System.Text.Json;
using StackExchange.Redis;

namespace Infrastructure.Caches;

public class RedisCacher(IConnectionMultiplexer redis) : ICacher
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _db.KeyDeleteAsync(key);
}
