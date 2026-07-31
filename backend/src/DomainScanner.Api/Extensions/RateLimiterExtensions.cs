using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using DomainScanner.Contracts.Models;
using DomainScanner.Contracts.Options;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring API rate limiting.
/// </summary>
public static class RateLimiterExtensions
{
    private const string GlobalScanPartition = "global-scan";
    private const string NonScanPartition = "non-scan";

    /// <summary>
    /// Adds and configures rate limiting policies used by the API,
    /// including per-client sliding-window limits and a global
    /// concurrency limit for scan endpoints.
    /// </summary>
    /// <param name="services">
    /// The service collection to which rate limiting services are added.
    /// </param>
    /// <returns>
    /// The same service collection instance for further configuration.
    /// </returns>
    public static IServiceCollection AddAndConfigureRateLimiter(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Reject model
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (rejected, cancellationToken) =>
            {
                var context = rejected.HttpContext;
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (rejected.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out var retryAfter))
                {
                    context.Response.Headers.RetryAfter =
                        Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);

                    var logger = context.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("RateLimiting");

                    logger.LogWarning(
                        "Rate limit exceeded for {Method} {Path}. TraceId: {TraceId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier
                    );

                    await context.Response.WriteAsJsonAsync(
                        new ErrorResponse
                        {
                            StatusCode = StatusCodes.Status429TooManyRequests,
                            Message = "Too many requests. Please try again later"
                        }, cancellationToken);
                }
            };

            // Read
            options.AddPolicy(
                RateLimitingOptions.Read,
                context => CreateSlidingWindowPartition(
                    context,
                    permitLimit: 100)
            );

            // Write
            options.AddPolicy(
                RateLimitingOptions.Write,
                context => CreateSlidingWindowPartition(
                    context,
                    permitLimit: 20)
            );

            // Auth
            options.AddPolicy(
                RateLimitingOptions.Auth,
                context => CreateSlidingWindowPartition(
                    context,
                    permitLimit: 5)
            );

            // Login
            options.AddPolicy(
                RateLimitingOptions.Login,
                context => CreateSlidingWindowPartition(
                    context,
                    permitLimit: 10)
            );

            // Scan
            options.AddPolicy(
                RateLimitingOptions.Scan,
                context => CreateSlidingWindowPartition(context, 15));

            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    if (!IsScanEndpoint(context))
                    {
                        return RateLimitPartition.GetNoLimiter("no-limit")!;
                    }

                    var key = GetClientPartitionKey(context);

                    return RateLimitPartition.GetConcurrencyLimiter(
                        key,
                        _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = 5,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        })!;
                });
        });

        return services;
    }

    /// <summary>
    /// Determines whether the current request targets an endpoint
    /// configured with the scan rate limiting policy.
    /// </summary>
    /// <param name="context">
    /// The HTTP context associated with the current request.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the selected endpoint uses the scan
    /// rate limiting policy; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsScanEndpoint(HttpContext context)
    {
        var attribute = context
            .GetEndpoint()?
            .Metadata
            .GetMetadata<EnableRateLimitingAttribute>();

        return attribute?.PolicyName == RateLimitingOptions.Scan;
    }

    /// <summary>
    /// Creates a sliding-window rate limit partition for the current client.
    /// </summary>
    /// <param name="context">
    /// The HTTP context used to resolve the client partition key.
    /// </param>
    /// <param name="permitLimit">
    /// The maximum number of requests permitted within the rate limiting window.
    /// </param>
    /// <returns>
    /// A sliding-window rate limit partition associated with the current client.
    /// </returns>
    private static RateLimitPartition<string?> CreateSlidingWindowPartition(
        HttpContext context,
        int permitLimit)
    {
        var partitionKey = GetClientPartitionKey(context);

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey,
            _ => ConfigureFactory(
                permitLimit: permitLimit,
                minutes: 1,
                segments: 6,
                queueLimit: 0,
                order: QueueProcessingOrder.OldestFirst
            ));
    }

    /// <summary>
    /// Resolves the partition key used to isolate rate limits between clients.
    /// </summary>
    /// <param name="context">
    /// The HTTP context associated with the current request.
    /// </param>
    /// <returns>
    /// The authenticated user's identifier when available; otherwise,
    /// the remote IP address or the <c>anonymous</c> fallback value.
    /// </returns>
    private static string? GetClientPartitionKey(HttpContext context)
    {
        return context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? context.Connection.RemoteIpAddress?.ToString()
               ?? "anonymous";
    }

    /// <summary>
    /// Creates sliding-window rate limiter options.
    /// </summary>
    /// <param name="permitLimit">
    /// The maximum number of permits available within the configured window.
    /// </param>
    /// <param name="minutes">
    /// The duration of the rate limiting window, in minutes.
    /// </param>
    /// <param name="segments">
    /// The number of segments into which the window is divided.
    /// </param>
    /// <param name="queueLimit">
    /// The maximum number of requests that may wait for a permit.
    /// </param>
    /// <param name="order">
    /// The order in which queued requests are processed.
    /// </param>
    /// <returns>
    /// Configured sliding-window rate limiter options.
    /// </returns>
    private static SlidingWindowRateLimiterOptions ConfigureFactory(
        int permitLimit,
        int minutes,
        int segments,
        int queueLimit,
        QueueProcessingOrder order = QueueProcessingOrder.OldestFirst)
    {
        return new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(minutes),
            SegmentsPerWindow = segments,
            QueueLimit = queueLimit,
            QueueProcessingOrder = order
        };
    }
}