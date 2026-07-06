using DomainScanner.Application.Abstractions.Cache;
using DomainScanner.Application.Pipelines.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Pipelines.Behaviors;

public class CachingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : ICacheable
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

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        _logger.LogInformation("Cache requested");

        string key = _cacheKeyGenerator.GenerateKey(request);

        _logger.LogInformation($"Key: {key}. Getting...");

        var cached = await _cache.GetAsync<TResponse>(key);

        _logger.LogInformation($"Get: {cached}");

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

        _logger.LogInformation($"Cache setted: {key}, {response}");

        return response;
    }
}