using DomainScanner.Contracts.Options.Worker;
using Hangfire;

namespace DomainScanner.Worker.Extensions;

/// <summary>
/// Provides extensions methods for configuring Hangfire worker services and options.
/// </summary>
public static class HangfireServiceExtensions
{
    /// <summary>
    /// Binds the required worker section and validates scheduling, processing, and retention settings at startup.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">App configuration.</param>
    public static IServiceCollection ConfigureWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DomainChecksWorkerOptions>()
            .Bind(configuration.GetRequiredSection(
                DomainChecksWorkerOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.RecurringJobId),
                "Recurring job ID is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.CronExpression),
                "Cron expression is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.QueueName),
                "Queue name is required.")
            .Validate(
                options => options.BatchSize > 0,
                "Batch size must be greater than zero.")
            .Validate(
                options => options.MaxDomainsPerRun > 0,
                "Maximum domains per run must be greater than zero.")
            .Validate(
                options => options.RetentionDays > 0,
                "Retention days must be greater than zero.")
            .Validate(
                options =>
                    options.MaxResultsPerDomain > 0,
                "Maximum result count must be greater than zero.")
            .ValidateOnStart();

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
    /// Registers a Hangfire server listening on the configured domain-check queue.
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
