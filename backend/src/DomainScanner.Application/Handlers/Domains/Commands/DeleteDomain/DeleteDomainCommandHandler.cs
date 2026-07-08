using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;

/// <summary>
/// Handles <see cref="DeleteDomainCommand"/>. 
/// </summary>
public class DeleteDomainCommandHandler : IRequestHandler<DeleteDomainCommand, Unit>
{
    private readonly IRepository<DomainEntity, Guid> _repository;

    public DeleteDomainCommandHandler(IRepository<DomainEntity, Guid> repository)
    {
        _repository = repository;
    }
    
    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteDomainCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _repository.FindAsync(request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }
        
        // Deleting domain
        _repository.Delete(domain);
        
        return Unit.Value;
    }
}