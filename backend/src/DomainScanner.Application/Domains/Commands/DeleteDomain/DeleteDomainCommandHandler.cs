using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Commands.DeleteDomain;

public class DeleteDomainCommandHandler(IDomainsRepository domainsRepository, IUnitOfWork unitOfWork) 
    : IRequestHandler<DeleteDomainCommand, Guid>
{
    private readonly IDomainsRepository _domainsRepository = domainsRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<Guid> Handle(DeleteDomainCommand request, CancellationToken ct)
    {
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        if(domain is null)
            throw new DomainNotFoundException(nameof(DomainEntity), request.Id);
        
        _domainsRepository.Delete(domain);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return domain.Id;
    }
}