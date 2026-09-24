using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Mapping;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;

/// <summary>Retrieves an owned domain with a summary of its latest check.</summary>
public sealed class GetDomainByIdQueryHandler
    : IRequestHandler<GetDomainByIdQuery, DomainResponse>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IReadRepository<DomainCheckResult, Guid> _checks;

    /// <summary>Creates the query handler.</summary>
    public GetDomainByIdQueryHandler(
        IOwnedDomainProvider ownedDomains,
        IReadRepository<DomainCheckResult, Guid> checks)
    {
        _ownedDomains = ownedDomains;
        _checks = checks;
    }

    /// <inheritdoc />
    public async Task<DomainResponse> Handle(
        GetDomainByIdQuery request,
        CancellationToken ct)
    {
        var domain = await _ownedDomains.GetRequiredAsync(request.Id, ct);
        var checks = await _checks.GetAllWhereAsync(
            check => check.DomainId == domain.Id, ct);
        var latest = checks.OrderByDescending(check => check.CreatedAt)
            .ThenByDescending(check => check.Id)
            .FirstOrDefault();

        return new DomainResponse(
            domain.Id,
            domain.Address,
            domain.MonitoringEnabled,
            domain.UserId,
            latest is null ? null : DomainResponseMapping.ToSummary(latest));
    }
}
