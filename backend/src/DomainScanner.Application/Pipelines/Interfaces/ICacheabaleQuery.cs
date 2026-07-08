namespace DomainScanner.Application.Pipelines.Interfaces;

/// <summary>
/// Marks query as cacheable. Implements in <see cref="CachingBehavior{TRequest, TResponse}"/>.
/// </summary>
public interface ICacheableQuery;