using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Queries.GetAllDomains;

public class GetAllDomainsQueryHandler(IDomainsRepository domainsRepository) : IRequestHandler<GetAllDomainsQuery, List<DomainEntity>>
{
    private readonly IDomainsRepository _domainsRepository = domainsRepository;
    
    public async Task<List<DomainEntity>> Handle(GetAllDomainsQuery request, CancellationToken ct)
    {
        var domains = await _domainsRepository.GetAllAsync(ct);
        return domains;
    }
}