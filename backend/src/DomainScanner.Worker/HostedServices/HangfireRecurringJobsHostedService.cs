using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.Worker;
using DomainScanner.Shared.Hangfire.Interfaces;
using Hangfire;
using Microsoft.Extensions.Options;
#pragma warning disable CS0618 // Type or member is obsolete

namespace DomainScanner.Worker.HostedServices;

/// <summary>
/// Hosted service that registers and manages Hangfire recuuring jobs for domain checks. Implements <see cref="IHostedService"/>. 
/// </summary>
public class HangfireRecurringJobsHostedService : IHostedService
{
    private readonly ILogger<HangfireRecurringJobsHostedService> _logger;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly DomainChecksWorkerOptions _options;

    public HangfireRecurringJobsHostedService(
        ILogger<HangfireRecurringJobsHostedService> logger,
        IRecurringJobManager recurringJobManager,
        IOptions<DomainChecksWorkerOptions> options)
    {
        _logger = logger;
        _recurringJobManager = recurringJobManager;
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken ct)
    {
        _recurringJobManager.AddOrUpdate<IDomainsCheckJob>(
            _options.RecurringJobId,
            job =>  job.RunAsync(ct),
            _options.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc,
                QueueName = _options.QueueName,
                MisfireHandling = MisfireHandlingMode.Relaxed
            });

        _logger.LogInformation(
            "Hangfire recurring job registered. JobId={JobId}, Cron={Cron}, Queue={Queue}",
            _options.RecurringJobId,
            _options.CronExpression,
            _options.QueueName);

        return Task.CompletedTask;
    }
    
    /// <inheritdoc />
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}