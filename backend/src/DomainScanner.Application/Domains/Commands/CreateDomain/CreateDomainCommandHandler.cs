using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Commands.CreateDomain;

public class CreateDomainCommandHandler(IDomainsRepository domainsRepository, IUnitOfWork  unitOfWork) 
    : IRequestHandler<CreateDomainCommand, Guid>
{
    private readonly IDomainsRepository _domainsRepository = domainsRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<Guid> Handle(CreateDomainCommand request, CancellationToken ct)
    {
        
        await _domainsRepository.CreateAsync(request.Domain, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return request.Domain.Id;
    }
}