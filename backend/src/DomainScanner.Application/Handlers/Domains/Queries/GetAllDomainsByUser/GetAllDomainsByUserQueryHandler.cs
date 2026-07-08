using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetAllDomainsByUser;

/// <summary>
/// Handles <see cref="GetAllDomainsByUser"/> 
/// </summary>
public class GetAllDomainsByUserQueryHandler : IRequestHandler<GetAllDomainsByUserQuery, IEnumerable<DomainResponse>>
{
    private readonly IReadRepository<DomainEntity, Guid> _repository;
    private readonly IMapper _mapper;

    public GetAllDomainsByUserQueryHandler(IReadRepository<DomainEntity, Guid> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DomainResponse>> Handle(GetAllDomainsByUserQuery request, CancellationToken ct)
    {
        var domains = await _repository.GetAllWhereAsync(d => d.UserId == request.UserId, ct);
        return domains.Select(_mapper.Map<DomainResponse>);
    }
}