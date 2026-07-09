using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DomainScanner.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring health checks and monitoring endpoints.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds health checks for application dependencies to the service collection.
    /// </summary>
    public static IServiceCollection AddHealthCheck(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("PostgresConnection")!,
                name: "postgres",
                failureStatus: HealthStatus.Degraded
            );
        return services;
    }
    
    /// <summary>
    /// Maps health check endpoints to the application pipeline.
    /// </summary>
    public static WebApplication UseHealthCheck(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}