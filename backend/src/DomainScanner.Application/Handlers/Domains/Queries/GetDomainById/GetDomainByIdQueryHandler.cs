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
/// Handles <see cref="GetDomainByIdQuery"/> by retrieving an owned domain and its check history.
/// </summary>
public class GetDomainByIdQueryHandler : IRequestHandler<GetDomainByIdQuery, DomainResponse>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IReadRepository<DomainCheckResult, Guid> _checkRepository;

    public GetDomainByIdQueryHandler(
        IOwnedDomainProvider ownedDomains,
        IReadRepository<DomainCheckResult, Guid> checkRepository)
    {
        _ownedDomains = ownedDomains;
        _checkRepository = checkRepository;
    }

    /// <inheritdoc />
    public async Task<DomainResponse> Handle(
        GetDomainByIdQuery request,
        CancellationToken ct)
    {
        var domain = await _ownedDomains.GetRequiredAsync(
            request.Id,
            ct);

        var checks = await _checkRepository.GetAllWhereAsync(
            check => check.DomainId == domain.Id,
            ct);

        return new DomainResponse(
            domain.Id,
            domain.Address,
            domain.MonitoringEnabled,
            domain.UserId,
            checks
                .OrderByDescending(check => check.CreatedAt)
                .ThenByDescending(check => check.Id)
                .Select(check => new HttpResponse(
                    check.Address,
                    check.StatusCode,
                    check.IsActive,
                    check.CreatedAt))
                .ToArray());
    }
}
