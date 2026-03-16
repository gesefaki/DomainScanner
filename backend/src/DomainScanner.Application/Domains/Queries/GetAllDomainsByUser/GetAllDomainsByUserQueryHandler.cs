using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Queries.GetAllDomainsByUser;

public class GetAllDomainsByUserQueryHandler : IRequestHandler<GetAllDomainsByUserQuery, List<DomainEntity>>
{
    private readonly IDomainsRepository _repository;
    private readonly ILogger<GetAllDomainsByUserQueryHandler> _logger;

    public GetAllDomainsByUserQueryHandler(IDomainsRepository repository,
        ILogger<GetAllDomainsByUserQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<DomainEntity>> Handle(GetAllDomainsByUserQuery request, CancellationToken ct)
    {
        _logger.LogInformation($"Getting domains by userId {request.UserId}..");

        var domains = await _repository.GetAllByUserIdAsync(request.UserId, ct);
        
        _logger.LogInformation($"Domains found:  {domains.Count}.");
        
        return domains;
    }
    
    
    
}