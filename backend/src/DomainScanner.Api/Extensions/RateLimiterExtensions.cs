using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using DomainScanner.Contracts.Options;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddAndConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitingOptions.Read, context =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ConfigurePartitionKey(),
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
                    partitionKey: ConfigurePartitionKey(),
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
                    partitionKey: ConfigurePartitionKey(),
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
        });

        return services;
    }

    private static string? ConfigurePartitionKey()
    {
        return JwtRegisteredClaimNames.Sub;
    }

    private static string? ConfigurePartitionKey(HttpContext context)
    {
        return context!.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }

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