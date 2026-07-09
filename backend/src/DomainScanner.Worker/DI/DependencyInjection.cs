using DomainScanner.Shared.Hangfire.Interfaces;
using DomainScanner.Worker.Extensions;
using DomainScanner.Worker.HostedServices;
using DomainScanner.Worker.Jobs;
using Hangfire;
using Hangfire.PostgreSql;

namespace DomainScanner.Worker.DI;

/// <summary>
/// Provides extension methods for configuring Hangfire background job processing.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configures Hangfire.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">App configuration.</param>
    public static IServiceCollection AddWorker(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfire(conf => conf
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(opt =>
            opt.UseNpgsqlConnection(configuration.GetConnectionString("PostgresConnection"))));

        return services;
    }

    /// <summary>
    /// Configures the Hangfire worker server with recurring job scheduling.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">App configuration.</param>
    public static IServiceCollection AddWorkerServerExtension(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.ConfigureWorker(configuration);
        services.AddWorkerServer(configuration);

        services.AddHostedService<HangfireRecurringJobsHostedService>();

        services.AddScoped<IDomainsCheckJob, DomainChecksHangfireJob>();
        return services;
    }
}