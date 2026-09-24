using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Mapping;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainCheckById;

/// <summary>Checks ownership and maps one stored check.</summary>
public sealed class GetDomainCheckByIdQueryHandler
    : IRequestHandler<GetDomainCheckByIdQuery, DomainCheckResponse>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IReadRepository<DomainCheckResult, Guid> _checks;

    /// <summary>Creates the query handler.</summary>
    public GetDomainCheckByIdQueryHandler(
        IOwnedDomainProvider ownedDomains,
        IReadRepository<DomainCheckResult, Guid> checks)
    {
        _ownedDomains = ownedDomains;
        _checks = checks;
    }

    /// <inheritdoc />
    public async Task<DomainCheckResponse> Handle(
        GetDomainCheckByIdQuery request, CancellationToken ct)
    {
        await _ownedDomains.GetRequiredAsync(request.DomainId, ct);
        var check = await _checks.GetAsync(
            item => item.Id == request.CheckId &&
                    item.DomainId == request.DomainId, ct);
        return check is null
            ? throw new DomainCheckNotFoundException(request.CheckId)
            : DomainResponseMapping.ToCheck(check);
    }
}
