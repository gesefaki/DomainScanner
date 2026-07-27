using DomainScanner.Application.Abstractions.Cache;
using DomainScanner.Application.Pipelines.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Pipelines.Behaviors;

/// <summary>
/// Pipeline behavior responsible for caching queries.
/// </summary>
/// <typeparam name="TRequest">Type of the request.</typeparam>
/// <typeparam name="TResponse">Type of the response.</typeparam>
public sealed class CachingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : ICacheableQuery
{
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator<TRequest> _cacheKeyGenerator;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cache,
     ICacheKeyGenerator<TRequest> cacheKeyGenerator,
     ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _cacheKeyGenerator = cacheKeyGenerator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        _logger.LogInformation("Cache requested");

        string key = _cacheKeyGenerator.GenerateKey(request);

        var cached = await _cache.GetAsync<TResponse>(key);

        if (cached != null)
        {
            _logger.LogInformation("Cache is not null, returning");
            return cached;
        }

        var response = await next(ct);

        await _cache.SetAsync(
            key,
            response
        );

        _logger.LogInformation("Cache setted: {Key}, {Response}", key, response);

        return response;
    }
}