using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;

public class DeleteDomainCommandHandler : IRequestHandler<DeleteDomainCommand, Unit>
{
    private readonly IReadRepository<DomainEntity> _readRepository;
    private readonly IWriteRepository<DomainEntity> _writeRepository;

    public DeleteDomainCommandHandler(IReadRepository<DomainEntity> readRepository, 
        IWriteRepository<DomainEntity> writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }
     
    public async Task<Unit> Handle(DeleteDomainCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _readRepository.FindAsync(request.Request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Request.Id);
        }
        
        // Deleting domain
        _writeRepository.Delete(domain);
        
        return Unit.Value;
    }
}