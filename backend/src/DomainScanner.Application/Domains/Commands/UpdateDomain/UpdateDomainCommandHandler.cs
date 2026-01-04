using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Commands.UpdateDomain;


public class UpdateDomainCommandHandler : IRequestHandler<UpdateDomainCommand, Guid>
{
    private readonly ILogger<UpdateDomainCommandHandler> _logger;
    private readonly IUnitOfWork _uof;
    private readonly IDomainsRepository _domainsRepository;

    public UpdateDomainCommandHandler(ILogger<UpdateDomainCommandHandler> logger,
        IUnitOfWork uof,
        IDomainsRepository domainsRepository)
    {
        _logger = logger;
        _uof = uof;
        _domainsRepository = domainsRepository;
    }
    
    public async Task<Guid> Handle(UpdateDomainCommand request, CancellationToken ct)
    {
        _logger.LogInformation($"Getting domain with id {request.Id}...");
        
        // Getting domain
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        if (domain is null)
        {
            _logger.LogWarning($"Domain with id {request.Id} not found");
            throw new DomainNotFoundException(nameof(DomainEntity), request.Id);
        }

        _logger.LogInformation($"Domain  with id {request.Id} was found.");

        _logger.LogInformation($"Updating domain with id {request.Id}...");
        // Updating domain
        _domainsRepository.Update(domain);
        await _uof.SaveChangesAsync(ct);
        
        _logger.LogInformation($"Domain with id {request.Id} has been updated");
        
        return domain.Id;
    }
}