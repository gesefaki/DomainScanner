using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DomainScanner.Api.Extensions;

public static class HealthCheckExtensions
{
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

    public static WebApplication UseHealthCheck(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}