using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Contracts.Options;
using DomainScanner.Domain.Entities;
using DomainScanner.Shared.Hangfire.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace DomainScanner.Worker.Jobs;

/// <summary>
/// Hangfire background job that permorms HTTP checks on a batch of domains. Implements <see cref="IDomainsCheckJob"/>.
/// </summary>
public class DomainChecksHangfireJob : IDomainsCheckJob
{
    private readonly IReadRepository<DomainEntity, Guid> _readRepository;
    private readonly ILogger<DomainChecksHangfireJob> _logger;
    private readonly IMediator _mediator;
    private readonly DomainChecksWorkerOptions _options;

    public DomainChecksHangfireJob(
        IReadRepository<DomainEntity, Guid> readRepository,
        ILogger<DomainChecksHangfireJob> logger,
        IMediator mediator,
        IOptions<DomainChecksWorkerOptions> options)
    {
        _readRepository = readRepository;
        _logger = logger;
        _mediator = mediator;
        _options = options.Value;
    }

    /// <summary>
    /// Executes the domain check job by processing a batch of domains.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunAsync(CancellationToken ct)
    {
        var domainsBatch = (List<DomainEntity>)await _readRepository.GetBatchAsync(_options.BatchSize, ct);

        _logger.LogInformation($"Worker started. Domains batch size: {domainsBatch.Count}");

        foreach (var domain in domainsBatch)
        {
            try
            {
                await _mediator.Send(new HttpSendAndSaveCommand(domain.Id), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during domain {domain.Id} with address {domain.Address}: {ex.Message}");
            }
        }
    }
}