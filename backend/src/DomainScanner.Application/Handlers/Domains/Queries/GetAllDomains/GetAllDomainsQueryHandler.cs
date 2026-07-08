using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetAllDomains;

/// <summary>
/// Handles <see cref="GetAllDomainsQuery"/> 
/// </summary>
public class GetAllDomainsQueryHandler : IRequestHandler<GetAllDomainsQuery, IEnumerable<DomainResponse>>
{
    private readonly IReadRepository<DomainEntity, Guid> _repository;
    private readonly IMapper _mapper;

    public GetAllDomainsQueryHandler(IReadRepository<DomainEntity, Guid> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DomainResponse>> Handle(GetAllDomainsQuery request, CancellationToken ct)
    {
        var domains = await _repository.GetAllAsync(ct);
        return domains.Select(_mapper.Map<DomainResponse>);
    }
}