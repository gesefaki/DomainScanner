using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Mapping;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainChecks;

/// <summary>Checks ownership and maps retained checks in newest-first order.</summary>
public sealed class GetDomainChecksQueryHandler
    : IRequestHandler<GetDomainChecksQuery, IReadOnlyList<DomainCheckResponse>>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IReadRepository<DomainCheckResult, Guid> _checks;

    /// <summary>Creates the query handler.</summary>
    public GetDomainChecksQueryHandler(
        IOwnedDomainProvider ownedDomains,
        IReadRepository<DomainCheckResult, Guid> checks)
    {
        _ownedDomains = ownedDomains;
        _checks = checks;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DomainCheckResponse>> Handle(
        GetDomainChecksQuery request, CancellationToken ct)
    {
        await _ownedDomains.GetRequiredAsync(request.DomainId, ct);
        var checks = await _checks.GetAllWhereAsync(
            check => check.DomainId == request.DomainId, ct);
        return checks.OrderByDescending(check => check.CreatedAt)
            .ThenByDescending(check => check.Id)
            .Select(DomainResponseMapping.ToCheck)
            .ToArray();
    }
}
