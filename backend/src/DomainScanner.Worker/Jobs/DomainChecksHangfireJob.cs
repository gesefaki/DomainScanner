using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Domain.Entities;
using DomainScanner.Shared.Hangfire.Interfaces;
using DomainScanner.Worker.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace DomainScanner.Worker.Jobs;

public class DomainChecksHangfireJob : IDomainsCheckJob
{
    private readonly IReadRepository<DomainEntity> _readRepository;
    private readonly ILogger<DomainChecksHangfireJob> _logger;
    private readonly IMediator _mediator;
    private readonly DomainChecksWorkerOptions _options;

    public DomainChecksHangfireJob(
        IReadRepository<DomainEntity> readRepository,
        ILogger<DomainChecksHangfireJob> logger,
        IMediator mediator,
        IOptions<DomainChecksWorkerOptions> options)
    {
        _readRepository = readRepository;
        _logger = logger;
        _mediator = mediator;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var domainsBatch = (List<DomainEntity>)await _readRepository.GetBatchAsync(_options.BatchSize, ct);

        _logger.LogInformation($"Worker started. Domains batch size: {domainsBatch.Count}");

        foreach (var domain in domainsBatch)
        {
            try
            {
                _logger.LogInformation($"Domain {domain.Id} with address {domain.Address} is being found");
                await _mediator.Send(new HttpSendAndSaveCommand(domain.Id), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during domain {domain.Id} with address {domain.Address}: {ex.Message}");
            }
        }
    }
}