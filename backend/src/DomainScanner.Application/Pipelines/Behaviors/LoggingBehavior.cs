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
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next(ct);

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedTime} ms",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}