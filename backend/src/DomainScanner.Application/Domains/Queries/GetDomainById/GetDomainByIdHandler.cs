using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Domains.Exceptions;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Queries.GetDomainById;

public class GetDomainByIdHandler(IDomainsRepository domainsRepository) 
    : IRequestHandler<GetDomainByIdQuery, DomainEntity?>
{
    private readonly IDomainsRepository _domainsRepository = domainsRepository;
    
    
    public async Task<DomainEntity?> Handle(GetDomainByIdQuery request, CancellationToken ct)
    {
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        return domain;
    }
}