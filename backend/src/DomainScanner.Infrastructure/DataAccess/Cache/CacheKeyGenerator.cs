using System.Text.Json;
using DomainScanner.Application.Abstractions.Cache;

namespace DomainScanner.Infrastructure.DataAccess.Cache;

public class CacheKeyGenerator<T> : ICacheKeyGenerator<T> where T : notnull
{
    public string GenerateKey(T request)
    {
        var key = $"{typeof(T).Name}:{JsonSerializer.Serialize(request)}";
        return key;
    }
}