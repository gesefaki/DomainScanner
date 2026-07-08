namespace DomainScanner.Application.Abstractions.Cache;

/// <summary>
/// Define contract for generating cache keys for objects of type <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T">The type of object for which cache keys are generated. Must be a non-nullable type.</typeparam>
public interface ICacheKeyGenerator<T> where T : notnull
{
    /// <summary>
    /// Generates a unique cache key for the specified request object.
    /// </summary>
    /// <param name="request">The request object to generate a cache key for.</param>
    string GenerateKey(T request);
}