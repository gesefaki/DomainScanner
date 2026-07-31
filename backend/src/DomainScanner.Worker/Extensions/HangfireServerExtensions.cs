using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.Worker;
using Hangfire;

namespace DomainScanner.Worker.Extensions;

/// <summary>
/// Provides extensions methods for configuring Hangfire worker services and options.
/// </summary>
public static class HangfireServiceExtensions
{
    /// <summary>
    /// Configures domain checks worker options from the app configuration.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">App configuration.</param>
    public static IServiceCollection ConfigureWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DomainChecksWorkerOptions>(
            configuration.GetSection(DomainChecksWorkerOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Retrieves the domain checks worker options from configuration.
    /// </summary>
    /// <param name="configuration">App configuration.</param>
    /// <returns>A <see cref="DomainChecksWorkerOptions"/> instance with values from configuration, or a new instance with default values if the conf section is missing.</returns>
    private static DomainChecksWorkerOptions GetWorkerOptions(IConfiguration configuration)
    {
        return configuration
            .GetSection(DomainChecksWorkerOptions.SectionName)
            .Get<DomainChecksWorkerOptions>() ?? new DomainChecksWorkerOptions();
    }

    /// <summary>
    /// Retrieves the domain checks worker options from configuration.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">App configuration.</param>
    public static IServiceCollection AddWorkerServer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfireServer(options =>
        {
            options.ServerName = $"domains-check-worker-{Environment.MachineName}";
            options.Queues = [GetWorkerOptions(configuration).QueueName];
        });

        return services;
    }
}