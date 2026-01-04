using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Queries.GetAllDomains;

public class GetAllDomainsQueryHandler : IRequestHandler<GetAllDomainsQuery, List<DomainEntity>>
{
    private readonly IDomainsRepository _domainsRepository;
    private readonly ILogger<GetAllDomainsQueryHandler> _logger;

    public GetAllDomainsQueryHandler(IDomainsRepository domainsRepository, 
        ILogger<GetAllDomainsQueryHandler> logger)
    {
        _domainsRepository = domainsRepository;
        _logger = logger;
    }
    
    public async Task<List<DomainEntity>> Handle(GetAllDomainsQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting all of domains...");
        var domains = await _domainsRepository.GetAllAsync(ct);
        
        _logger.LogInformation($"Domains found: {domains.Count}");
        return domains;
    }
}