using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Commands.DeleteDomain;

public class DeleteDomainCommandHandler : IRequestHandler<DeleteDomainCommand, Guid>
{
    private readonly IDomainsRepository _domainsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDomainCommandHandler> _logger;
    
    public DeleteDomainCommandHandler(IDomainsRepository domainsRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteDomainCommandHandler> logger)
    {
        _domainsRepository =  domainsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(DeleteDomainCommand request, CancellationToken ct)
    {
        _logger.LogInformation($"Getting domain with id {request.Id}");

        // Getting domain
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        if (domain is null)
        {
            _logger.LogWarning($"Domain with id {request.Id} not found");
            throw new DomainNotFoundException(nameof(DomainEntity), request.Id);
        }
        _logger.LogInformation($"Domain  with id {request.Id} was find");

        _logger.LogInformation($"Deleting domain  with id {request.Id}...");
        // Deleting domain
        _domainsRepository.Delete(domain);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation($"Domain with id {request.Id} has been deleted");
        
        return domain.Id;
    }
}