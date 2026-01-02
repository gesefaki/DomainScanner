using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Domains.Exceptions;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Commands.UpdateDomain;

public class UpdateDomainCommandHandler(IDomainsRepository domainsRepository, IUnitOfWork unitOfWork) 
    : IRequestHandler<UpdateDomainCommand, Guid>
{
    private readonly IDomainsRepository _domainsRepository = domainsRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<Guid> Handle(UpdateDomainCommand request, CancellationToken ct)
    {
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        if (domain is null)
            throw new DomainNotFoundException(nameof(DomainEntity), request.Id);
        
        _domainsRepository.Update(domain);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return domain.Id;
    }
}