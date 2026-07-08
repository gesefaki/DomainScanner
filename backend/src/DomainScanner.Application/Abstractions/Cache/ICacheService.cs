namespace DomainScanner.Application.Abstractions.Cache;

/// <summary>
/// Defines the contract for a distributed caching services.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrivies a cached value by its key.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">The cache key to lookup.</param>
    /// <returns>A task representing the async operation that returns the cached value.</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Stores a value in the cache with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of value to store.</typeparam>
    /// <param name="key">The cache key to store the value under.</param>
    /// <param name="value">The value store in the cache.</param>
    Task SetAsync<T>(
        string key,
        T value
    );

    /// <summary>
    /// Removes a cached value by its key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks whether a cached value exists for the specified key.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <returns>A task representing the async operation that returns true if key exists, otherwise false.</returns>
    Task<bool> IsExistsAsync(string key);
}