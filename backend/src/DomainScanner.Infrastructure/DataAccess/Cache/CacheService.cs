using System.Text.Json;
using DomainScanner.Application.Abstractions.Cache;
using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.Cache;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DomainScanner.Infrastructure.DataAccess.Cache;

/// <summary>
/// Redis-based implementation of the cache service. Implements the <see cref="ICacheService"/> contract. 
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly CacheSettings _cacheSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer used to interact with Redis server.</param>
    /// <param name="cacheSettings">The cache configuration settings including default expiration times.</param>
    public RedisCacheService(IConnectionMultiplexer redis, IOptions<CacheSettings> cacheSettings)
    {
        _database = redis.GetDatabase();
        _cacheSettings = cacheSettings.Value;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        RedisValue value = await _database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return default;
        }

        var json = value.ToString();

        return JsonSerializer.Deserialize<T>(json);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    /// <inheritdoc />
    public async Task<bool> IsExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }
}