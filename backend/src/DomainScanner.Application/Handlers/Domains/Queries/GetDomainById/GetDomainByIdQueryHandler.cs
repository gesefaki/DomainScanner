using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.HTTPs.Responses;
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
    private readonly IReadRepository<DomainCheckResult, Guid> _checkRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public GetDomainByIdQueryHandler(
        IReadRepository<DomainEntity, Guid> repository,
        IReadRepository<DomainCheckResult, Guid> checkRepository,
        IMapper mapper,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _checkRepository = checkRepository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<DomainResponse> Handle(GetDomainByIdQuery request, CancellationToken ct)
    {
        var domain = await _repository.FindAsync(request.Id, ct);

        if (domain is null || domain.UserId != _currentUser.Id)
        {
            throw new DomainNotFoundException(request.Id);
        }

        var checks = await _checkRepository.GetAllWhereAsync(
            check => check.DomainId == domain.Id,
            ct);
        
        return new DomainResponse(
            domain.Id,
            domain.Address,
            domain.IsActive,
            domain.UserId,
            checks
                .OrderByDescending(check => check.CreatedAt)
                .ThenByDescending(check => check.Id)
                .Select(check =>
                    new HttpResponse(
                        check.Address,
                        check.StatusCode,
                        check.IsActive,
                        check.CreatedAt))
                .ToArray());
    }
}
