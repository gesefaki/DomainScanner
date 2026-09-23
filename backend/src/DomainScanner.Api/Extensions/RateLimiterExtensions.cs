using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using DomainScanner.Contracts.Models;
using DomainScanner.Contracts.Options.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.Extensions;

/// <summary>Registers per-client request quotas and per-client and process-wide scan concurrency limits.</summary>
public static class RateLimiterExtensions
{
    private const string GlobalScanPartition = "global-scan";
    private const string NonScanPartition = "non-scan";

    /// <summary>Configures validated rate limiting options and HTTP 429 responses.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration containing the RateLimiting section.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// Only endpoints marked with the scan policy acquire concurrency permits.
    /// The shared partition limits one API process, not background workers or other replicas.
    /// Client partitions use the authenticated subject, falling back to the remote IP address.
    /// </remarks>
    public static IServiceCollection AddAndConfigureRateLimiter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(
            RateLimitingSettings.SectionName);

        services
            .AddOptions<RateLimitingSettings>()
            .Bind(section)
            .Validate(
                settings => settings.IsValid(),
                "Rate limiting configuration is invalid.")
            .ValidateOnStart();

        var settings = section.Get<RateLimitingSettings>()
                       ?? throw new InvalidOperationException(
                           "Rate limiting configuration is missing.");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (
                rejected,
                cancellationToken) =>
            {
                var context = rejected.HttpContext;

                context.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                if (rejected.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out var retryAfter))
                {
                    context.Response.Headers.RetryAfter =
                        Math.Max(
                                1,
                                (int)Math.Ceiling(
                                    retryAfter.TotalSeconds))
                            .ToString(
                                CultureInfo.InvariantCulture);
                }

                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiting");

                logger.LogWarning(
                    "Rate limit exceeded for {Method} {Path}. " +
                    "TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                await context.Response.WriteAsJsonAsync(
                    new ErrorResponse
                    {
                        StatusCode =
                            StatusCodes.Status429TooManyRequests,
                        Message =
                            "Too many requests. Please try again later."
                    },
                    cancellationToken);
            };

            options.AddPolicy(
                RateLimitingSettings.Policies.Read,
                context => CreateSlidingWindowPartition(
                    context,
                    settings.Read));

            options.AddPolicy(
                RateLimitingSettings.Policies.Write,
                context => CreateSlidingWindowPartition(
                    context,
                    settings.Write));

            options.AddPolicy(
                RateLimitingSettings.Policies.Auth,
                context => CreateSlidingWindowPartition(
                    context,
                    settings.Auth));

            options.AddPolicy(
                RateLimitingSettings.Policies.Login,
                context => CreateSlidingWindowPartition(
                    context,
                    settings.Login));

            options.AddPolicy(
                RateLimitingSettings.Policies.Scan,
                context => CreateSlidingWindowPartition(
                    context,
                    settings.Scan));
            
            options.GlobalLimiter =
                PartitionedRateLimiter.CreateChained<HttpContext>(
                    CreateGlobalScanLimiter(
                        settings.GlobalScanConcurrency),
                    CreatePerClientScanLimiter(
                        settings.ScanConcurrency));
        });

        return services;
    }

    private static PartitionedRateLimiter<HttpContext>
        CreateGlobalScanLimiter(
            ConcurrencySettings settings)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
            {
                if (!IsScanEndpoint(context))
                {
                    return RateLimitPartition.GetNoLimiter(
                        NonScanPartition);
                }
                
                return RateLimitPartition.GetConcurrencyLimiter(
                    GlobalScanPartition,
                    _ => CreateConcurrencyOptions(settings));
            });
    }

    private static PartitionedRateLimiter<HttpContext>
        CreatePerClientScanLimiter(
            ConcurrencySettings settings)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
            {
                if (!IsScanEndpoint(context))
                {
                    return RateLimitPartition.GetNoLimiter(
                        NonScanPartition);
                }

                return RateLimitPartition.GetConcurrencyLimiter(
                    GetClientPartitionKey(context),
                    _ => CreateConcurrencyOptions(settings));
            });
    }

    private static ConcurrencyLimiterOptions
        CreateConcurrencyOptions(
            ConcurrencySettings settings)
    {
        return new ConcurrencyLimiterOptions
        {
            PermitLimit = settings.PermitLimit,
            QueueLimit = settings.QueueLimit,
            QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst
        };
    }

    private static RateLimitPartition<string>
        CreateSlidingWindowPartition(
            HttpContext context,
            SlidingWindowSettings settings)
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            GetClientPartitionKey(context),
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = settings.PermitLimit,
                Window =
                    TimeSpan.FromSeconds(
                        settings.WindowSeconds),
                SegmentsPerWindow =
                    settings.SegmentsPerWindow,
                QueueLimit = settings.QueueLimit,
                QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst
            });
    }

    private static bool IsScanEndpoint(
        HttpContext context)
    {
        var attribute = context
            .GetEndpoint()?
            .Metadata
            .GetMetadata<EnableRateLimitingAttribute>();

        return attribute?.PolicyName ==
               RateLimitingSettings.Policies.Scan;
    }

    private static string GetClientPartitionKey(
        HttpContext context)
    {
        return context.User.FindFirstValue(
                   JwtRegisteredClaimNames.Sub)
               ?? context.Connection.RemoteIpAddress?.ToString()
               ?? "anonymous";
    }
}
