using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using DomainScanner.Contracts.Options;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring API rate limiting.
/// </summary>
public static class RateLimiterExtensions
{
    /// <summary>
    /// Adds and configures rate limiting policies used by the API.
    /// </summary>
    /// <param name="services">
    /// The service collection to which rate limiting services are added.
    /// </param>
    /// <returns>
    /// The same service collection instance for further configuration.
    /// </returns>
    public static IServiceCollection AddAndConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitingOptions.Read, context =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ConfigurePartitionKey(context),
                    factory: _ => ConfigureFactory(
                        permitLimit: 100,
                        minutes: 1,
                        segments: 6,
                        queueLimit: 0)
                );
            });
        });

        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitingOptions.Write, context =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ConfigurePartitionKey(context),
                    factory: _ => ConfigureFactory(
                        permitLimit: 20,
                        minutes: 1,
                        segments: 6,
                        queueLimit: 0)
                );
            });
        });

        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitingOptions.Auth, context =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ConfigurePartitionKey(context),
                    factory: _ => ConfigureFactory(
                        permitLimit: 5,
                        minutes: 1,
                        segments: 6,
                        queueLimit: 0)
                );
            });
        });

        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitingOptions.Scan, context =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ConfigurePartitionKey(context),
                    factory: _ => ConfigureFactory(
                        permitLimit: 15,
                        minutes: 1,
                        segments: 6,
                        queueLimit: 0)
                );
            });
        });

        services.AddRateLimiter(options =>
        {
            options.AddConcurrencyLimiter(RateLimitingOptions.ConcurrencyScan, limiter =>
            {
                limiter.PermitLimit = 5;
                limiter.QueueLimit = 10;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
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
    private static string? ConfigurePartitionKey(HttpContext context)
    {
        return context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? context!.Connection.RemoteIpAddress?.ToString()
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
    /// <returns>
    /// Configured sliding-window rate limiter options.
    /// </returns>
    private static SlidingWindowRateLimiterOptions ConfigureFactory(
        int permitLimit,
        int minutes,
        int segments,
        int queueLimit
    )
    {
        return new SlidingWindowRateLimiterOptions()
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(minutes),
            SegmentsPerWindow = segments,
            QueueLimit = queueLimit
        };
    }
}