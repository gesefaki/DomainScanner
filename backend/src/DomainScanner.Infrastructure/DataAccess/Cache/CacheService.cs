using System.Text.Json;
using DomainScanner.Application.Abstractions.Cache;
using DomainScanner.Contracts.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DomainScanner.Infrastructure.DataAccess.Cache;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly CacheSettings _cacheSettings;

    public RedisCacheService(IConnectionMultiplexer redis, IOptions<CacheSettings> cacheSettings)
    {
        _database = redis.GetDatabase();
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        RedisValue value = await _database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(
        string key,
        T value)
    {
        var json = JsonSerializer.Serialize(value);

        var minutes = TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes);

        await _database.StringSetAsync(
            key,
            json,
            minutes
        );
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> IsExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }
}