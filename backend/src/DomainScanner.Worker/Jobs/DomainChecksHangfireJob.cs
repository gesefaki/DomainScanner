using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Domains.Commands.HttpSendAndSave;
using DomainScanner.Infrastructure.Hangfire.Options;
using DomainScanner.Shared.Hangfire.Interfaces;
using Microsoft.Extensions.Options;

namespace DomainScanner.Worker.Jobs;

public class DomainChecksHangfireJob : IDomainsCheckJob
{
    private readonly IDomainsRepository _domainsRepository;
    private readonly ILogger<DomainChecksHangfireJob> _logger;
    private readonly IMediator _mediator;
    private readonly DomainChecksWorkerOptions _options;
    
    public DomainChecksHangfireJob(
        IDomainsRepository domainsRepository,
        ILogger<DomainChecksHangfireJob> logger,
        IMediator mediator,
        IOptions<DomainChecksWorkerOptions> options)
    {
        _options = options.Value;
        _domainsRepository = domainsRepository;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var domainsBatch = await _domainsRepository.GetBatchAsync(_options.BatchSize, ct);

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