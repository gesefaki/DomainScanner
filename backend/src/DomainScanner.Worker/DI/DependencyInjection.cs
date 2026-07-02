using DomainScanner.Shared.Hangfire.Interfaces;
using DomainScanner.Worker.Extensions;
using DomainScanner.Worker.HostedServices;
using DomainScanner.Worker.Jobs;
using Hangfire;
using Hangfire.PostgreSql;

namespace DomainScanner.Worker.DI;

public static class DependencyInjection
{
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