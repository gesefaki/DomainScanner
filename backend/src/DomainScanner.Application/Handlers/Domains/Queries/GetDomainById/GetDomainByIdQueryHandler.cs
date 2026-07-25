using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;

/// <summary>
/// Handles <see cref="GetDomainByIdQuery"/> 
/// </summary>
public class GetDomainByIdQueryHandler : IRequestHandler<GetDomainByIdQuery, DomainResponse>
{
    private readonly IReadRepository<DomainEntity, Guid> _repository;
    private readonly IMapper _mapper;

    public GetDomainByIdQueryHandler(IReadRepository<DomainEntity, Guid> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<DomainResponse> Handle(GetDomainByIdQuery request, CancellationToken ct)
    {
        var domain = await _repository.FindAsync(request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }

        return _mapper.Map<DomainResponse>(domain);
    }
}