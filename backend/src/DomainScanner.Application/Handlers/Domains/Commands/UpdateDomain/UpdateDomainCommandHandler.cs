using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;

/// <summary>
/// Handles <see cref="UpdateDomainCommand"/>. Has a <see cref="UpdateDomainCommandValidator"/> must be passed. 
/// </summary>
public class UpdateDomainCommandHandler : IRequestHandler<UpdateDomainCommand, DomainResponse>
{
    private readonly IRepository<DomainEntity, Guid> _repository;
    private readonly IMapper _mapper;

    public UpdateDomainCommandHandler(IRepository<DomainEntity, Guid> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<DomainResponse> Handle(UpdateDomainCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _repository.FindAsync(request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }
        
        // Updating entity in-memory
        domain.Address = request.Request.Address;
        domain.IsActive = request.Request.IsActive;
        
        // Update in repository
        var updatedDomain = _repository.Update(domain);

        return _mapper.Map<DomainResponse>(updatedDomain);
    }
}