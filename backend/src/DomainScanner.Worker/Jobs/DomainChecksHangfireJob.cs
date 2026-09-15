using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Contracts.Options.Worker;
using DomainScanner.Domain.Entities;
using DomainScanner.Shared.Hangfire.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace DomainScanner.Worker.Jobs;

/// <summary>
/// Hangfire background job that performs and saves HTTP checks for all domains returned by the repository.
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
    /// <param name="options">The worker options that define the batch size.</param>
    /// <param name="scopeFactory">
    /// The factory for the identifier query scope and a separate scope for each domain check.
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
    /// Retrieves all domain identifiers and sends an <see cref="HttpSendAndSaveCommand"/>
    /// for each identifier, processing batches sequentially.
    /// </summary>
    /// <param name="ct">The cancellation token for loading identifiers and executing domain checks.</param>
    /// <returns>A task that completes when all retrieved identifiers have been processed.</returns>
    /// <remarks>
    /// The batch size controls grouping and does not limit the number of domains checked per run.
    /// Failures while loading identifiers propagate to the caller. Individual domain check failures
    /// are logged and processing continues, unless cancellation has been requested through <paramref name="ct"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// <see cref="DomainChecksWorkerOptions.BatchSize"/> is zero or negative.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Cancellation is requested through <paramref name="ct"/>.
    /// </exception>
    public async Task RunAsync(CancellationToken ct)
    {
        if (_options.BatchSize <= 0)
        {
            throw new InvalidOperationException("Domains checks batch size must be greater than zero");
        }

        IReadOnlyList<Guid> domainIds;

        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IReadRepository<DomainEntity, Guid>>();

            domainIds = await repository.GetIdsAsync(ct);
        }

        _logger.LogInformation("Starting checks for {DomainCount} domains.",
            domainIds.Count);

        foreach (var batch in domainIds.Chunk(_options.BatchSize))
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
                        new HttpSendAndSaveCommand(domainId),
                        ct);
                }
                catch (OperationCanceledException)
                    when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to check domain {DomainId}.",
                        domainId);
                }
            }
        }
    }
}
