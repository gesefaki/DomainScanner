using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;

/// <summary>
/// Handles <see cref="DeleteDomainCommand"/> by deleting a domain owned by the current authenticated user.
/// </summary>
public class DeleteDomainCommandHandler : IRequestHandler<DeleteDomainCommand, Unit>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IRepository<DomainEntity, Guid> _repository;

    public DeleteDomainCommandHandler(IOwnedDomainProvider ownedDomains,IRepository<DomainEntity, Guid> repository)
    {
        _ownedDomains = ownedDomains;
        _repository = repository;
    }
    
    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteDomainCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _ownedDomains.GetRequiredAsync(
            request.Id,
            ct);
        
        // Deleting domain
        _repository.Delete(domain);
        
        return Unit.Value;
    }
}
