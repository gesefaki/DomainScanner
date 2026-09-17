using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;

/// <summary>
/// Handles <see cref="UpdateDomainCommand"/> by updating a domain owned by the current authenticated user.
/// Has a <see cref="UpdateDomainCommandValidator"/> that must be passed.
/// </summary>
public class UpdateDomainCommandHandler : IRequestHandler<UpdateDomainCommand, DomainResponse>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IRepository<DomainEntity, Guid> _repository;
    private readonly IMapper _mapper;

    public UpdateDomainCommandHandler(
        IOwnedDomainProvider ownedDomains,
        IRepository<DomainEntity, Guid> repository,
        IMapper mapper)
    {
        _ownedDomains = ownedDomains;
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<DomainResponse> Handle(UpdateDomainCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _ownedDomains.GetRequiredAsync(
            request.Id,
            ct);
        
        // Updating entity in-memory
        domain.Address = request.Request.Address;
        domain.IsActive = request.Request.IsActive;
        
        // Update in repository
        var updatedDomain = _repository.Update(domain);

        return _mapper.Map<DomainResponse>(updatedDomain);
    }
}
