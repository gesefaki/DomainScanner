using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Services.Domains;
using DomainScanner.Shared.Hangfire.Interfaces;
using DomainScanner.Worker.Auth;
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
    /// Configures Hangfire storage and the domain check execution service.
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

        services.AddScoped<IDomainCheckExecutor, DomainCheckExecutor>();

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
        services.AddScoped<ICurrentUser, WorkerCurrentUser>();

        services.ConfigureWorker(configuration);
        services.AddWorkerServer(configuration);

        services.AddHostedService<HangfireRecurringJobsHostedService>();

        services.AddScoped<IDomainsCheckJob, DomainChecksHangfireJob>();
        return services;
    }
}
