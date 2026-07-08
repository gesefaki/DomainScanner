using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Pipelines.Behaviors;

/// <summary>
/// Pipeline behavior responsible for logging queries.
/// </summary>
/// <typeparam name="TRequest">Type of the request.</typeparam>
/// <typeparam name="TResponse">Type of the response.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling: {requestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(ct);

            _logger.LogInformation("Handled {requestName} in {elapsedTime} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Error when handling {requestName} in {elapsedTime} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}