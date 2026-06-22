using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;


public class UpdateDomainCommandHandler : IRequestHandler<UpdateDomainCommand, DomainResponse>
{
    private readonly IReadRepository<DomainEntity> _readRepository;
    private readonly IWriteRepository<DomainEntity> _writeRepository;
    private readonly IMapper _mapper;

    public UpdateDomainCommandHandler(IReadRepository<DomainEntity> readRepository, 
        IWriteRepository<DomainEntity> writeRepository, 
        IMapper mapper)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _mapper = mapper;
    }

    public async Task<DomainResponse> Handle(UpdateDomainCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _readRepository.FindAsync(request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }

        domain.Address = request.Request.Address;
        domain.IsActive = request.Request.IsActive;

        var updatedDomain = _writeRepository.Update(domain);

        return _mapper.Map<DomainResponse>(updatedDomain);
    }
}