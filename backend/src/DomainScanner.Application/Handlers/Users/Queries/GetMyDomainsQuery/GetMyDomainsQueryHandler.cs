using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetMyDomainsQuery;

/// <summary>
/// Handles <see cref="GetMyDomainsQuery"/>. 
/// </summary>
public class GetMyDomainsQueryHandler : IRequestHandler<GetMyDomainsQuery, IEnumerable<DomainResponse>>
{
    private readonly IReadRepository<DomainEntity, Guid> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public GetMyDomainsQueryHandler(IReadRepository<DomainEntity, Guid> repository,
        IMapper mapper,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DomainResponse>> Handle(GetMyDomainsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.Id;

        var domains = await _repository.GetAllWhereAsync(d => d.UserId == userId, ct);
        return domains.Select(_mapper.Map<DomainResponse>);
    }
}