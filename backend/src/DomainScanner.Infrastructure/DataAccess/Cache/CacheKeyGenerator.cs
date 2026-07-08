using System.Text.Json;
using DomainScanner.Application.Abstractions.Cache;

namespace DomainScanner.Infrastructure.DataAccess.Cache;

/// <summary>
/// Generates cache keys for objects of type <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T">The type of object for which cache keys are generated. Must be a non-nullable type.</typeparam>
public class CacheKeyGenerator<T> : ICacheKeyGenerator<T> where T : notnull
{
    /// <inheritdoc />
    public string GenerateKey(T request)
    {
        var key = $"{typeof(T).Name}:{JsonSerializer.Serialize(request)}";
        return key;
    }
}