using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Queries.GetDomainById;

public class GetDomainByIdQueryHandler : IRequestHandler<GetDomainByIdQuery, DomainEntity?>
{
    private readonly IDomainsRepository _domainsRepository;
    private readonly ILogger<GetDomainByIdQueryHandler> _logger;

    public GetDomainByIdQueryHandler(IDomainsRepository domainsRepository,
        ILogger<GetDomainByIdQueryHandler> logger)
    {
        _domainsRepository = domainsRepository;
        _logger = logger;
    }
    
    public async Task<DomainEntity?> Handle(GetDomainByIdQuery request, CancellationToken ct)
    {
        _logger.LogInformation($"Getting domain with  id {request.Id}...");
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        
        _logger.LogInformation($"Founded: {domain?.Id ?? null}");
        return domain;
    }
}