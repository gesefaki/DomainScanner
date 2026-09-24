using DomainScanner.Contracts.DTOs.HTTPs.Responses;

namespace DomainScanner.Contracts.DTOs.Domains.Responses;

/// <summary>A persisted check of a domain with a protocol-specific payload.</summary>
/// <param name="Id">Identifier of the stored check.</param>
/// <param name="DomainId">Identifier of the checked domain.</param>
/// <param name="Kind">Check protocol; currently <c>http</c>.</param>
/// <param name="CheckedAt">UTC time when the check completed.</param>
/// <param name="Outcome"><c>up</c>, <c>down</c>, or <c>error</c>.</param>
/// <param name="Http">HTTP data when <paramref name="Kind"/> is <c>http</c>.</param>
public sealed record DomainCheckResponse(
    Guid Id,
    Guid DomainId,
    string Kind,
    DateTime CheckedAt,
    string Outcome,
    HttpCheckResponse? Http);
