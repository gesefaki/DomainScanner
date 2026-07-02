using DomainScanner.Worker.Options;
using Hangfire;

namespace DomainScanner.Worker.Extensions;

public static class HangfireServiceExtensions
{
    public static IServiceCollection ConfigureWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DomainChecksWorkerOptions>(
            configuration.GetSection(DomainChecksWorkerOptions.SectionName));

        return services;
    }

    private static DomainChecksWorkerOptions GetWorkerOptions(IConfiguration configuration)
    {
        return configuration
            .GetSection(DomainChecksWorkerOptions.SectionName)
            .Get<DomainChecksWorkerOptions>() ?? new DomainChecksWorkerOptions();
    }

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