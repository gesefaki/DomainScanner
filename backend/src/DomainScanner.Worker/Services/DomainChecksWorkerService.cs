using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Domains.Commands.HttpSendAndSave;
using DomainScanner.Worker.Options;
using Microsoft.Extensions.Options;

namespace DomainScanner.Worker.Services;

public sealed class DomainChecksWorkerService : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly IDomainsRepository _domainsRepository;
    private readonly ILogger<DomainChecksWorkerService> _logger;
    private readonly DomainChecksWorkerOptions _options;

    public DomainChecksWorkerService(
        IMediator mediator,
        IDomainsRepository domainsRepository,
        ILogger<DomainChecksWorkerService> logger,
        IOptions<DomainChecksWorkerOptions> options)
    {
        _mediator = mediator;
        _domainsRepository = domainsRepository;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Starting worker job. Interval is {_options.IntervalSeconds} seconds, batch size is {_options.BatchSize}");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR]: {ex.Message}");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), ct);
        }
        
        _logger.LogInformation("Ending worker job.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        var domainsBatch =  await _domainsRepository.GetBatchAsync(_options.BatchSize, ct);
        _logger.LogInformation($"Processing {domainsBatch.Count} domains");

        foreach (var domain in domainsBatch)
        {
            ct.ThrowIfCancellationRequested();
            
            try
            {
                _logger.LogInformation($"Sending to domain {domain.Id} with address {domain.Address}...");
                await _mediator.Send(new HttpSendAndSaveCommand(domain.Id), ct);
                _logger.LogInformation($"Sending completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{domain.Id}:{domain.Address}] ERROR: {ex.Message}");
            }
        }
    }
}