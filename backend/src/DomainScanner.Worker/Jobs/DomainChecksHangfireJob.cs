using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Contracts.Options.Worker;
using DomainScanner.Shared.Hangfire.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace DomainScanner.Worker.Jobs;

/// <summary>
/// Checks a bounded set of eligible monitored domains, then prunes retained check history.
/// </summary>
/// <remarks>
/// Loads domain identifiers once per run and processes them sequentially in batches.
/// Each domain check uses a separate dependency injection scope.
/// </remarks>
public class DomainChecksHangfireJob : IDomainsCheckJob
{
    private readonly ILogger<DomainChecksHangfireJob> _logger;
    private readonly DomainChecksWorkerOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainChecksHangfireJob"/> class.
    /// </summary>
    /// <param name="logger">The logger for job progress and domain check failures.</param>
    /// <param name="options">Batch size, per-run domain limit, and history retention settings.</param>
    /// <param name="scopeFactory">
    /// The factory for separate identifier query, individual check, and history cleanup scopes.
    /// </param>
    public DomainChecksHangfireJob(
        ILogger<DomainChecksHangfireJob> logger,
        IOptions<DomainChecksWorkerOptions> options,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;

    }

    /// <summary>
    /// Retrieves at most the configured maximum of eligible identifiers, checks them sequentially,
    /// and applies age and count limits to check history.
    /// </summary>
    /// <param name="ct">The cancellation token for queries, domain checks, and history cleanup.</param>
    /// <returns>A task that completes after processing and history cleanup, including when no domains are eligible.</returns>
    /// <remarks>
    /// Batch size controls grouping, not concurrency; MaxDomainsPerRun bounds the selection.
    /// Failures while loading identifiers or pruning history propagate to the caller. Individual check failures
    /// are logged and processing continues, unless cancellation has been requested through <paramref name="ct"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="DomainChecksWorkerOptions.BatchSize"/> is zero or negative.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Cancellation is requested through <paramref name="ct"/>.
    /// </exception>
    public async Task RunAsync(
        CancellationToken ct)
    {
        IReadOnlyList<Guid> domainIds;

        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDomainScanRepository>();

            domainIds =
                await repository.GetMonitorableIdsAsync(
                    _options.MaxDomainsPerRun,
                    ct);
        }

        _logger.LogInformation(
            "Starting checks for {DomainCount} domains. " +
            "Maximum per run: {MaxDomainsPerRun}.",
            domainIds.Count,
            _options.MaxDomainsPerRun);

        var succeeded = 0;
        var failed = 0;

        foreach (var batch in
                 domainIds.Chunk(_options.BatchSize))
        {
            foreach (var domainId in batch)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    await using var scope =
                        _scopeFactory.CreateAsyncScope();

                    var sender = scope.ServiceProvider
                        .GetRequiredService<ISender>();

                    await sender.Send(
                        new HttpSendAndSaveCommand(
                            domainId),
                        ct);

                    succeeded++;
                }
                catch (OperationCanceledException)
                    when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failed++;

                    _logger.LogError(
                        ex,
                        "Failed to check domain {DomainId}.",
                        domainId);
                }
            }
        }

        await PruneCheckHistoryAsync(ct);

        _logger.LogInformation(
            "Domain checks finished. " +
            "Succeeded={Succeeded}, Failed={Failed}.",
            succeeded,
            failed);
    }

    private async Task PruneCheckHistoryAsync(
        CancellationToken ct)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var retention = scope.ServiceProvider
            .GetRequiredService<
                IDomainCheckRetentionService>();

        var deleteBefore = DateTime.UtcNow.AddDays(
            -_options.RetentionDays);

        await retention.PruneAsync(
            deleteBefore,
            _options.MaxResultsPerDomain,
            ct);

        _logger.LogInformation(
            "Check history pruned. " +
            "RetentionDays={RetentionDays}, " +
            "MaxResultsPerDomain={MaxResultsPerDomain}.",
            _options.RetentionDays,
            _options.MaxResultsPerDomain);
    }
}
