namespace DomainScanner.Contracts.DTOs.Domains.Responses;

/// <summary>A saved domain and a protocol-neutral summary of its latest check.</summary>
/// <param name="Id">The domain identifier.</param>
/// <param name="Address">The configured address.</param>
/// <param name="MonitoringEnabled">Whether scheduled monitoring is enabled.</param>
/// <param name="UserId">The owner's identifier.</param>
/// <param name="LastCheck">The latest check, or null before the first check.</param>
public sealed record DomainResponse(
    Guid Id,
    string Address,
    bool MonitoringEnabled,
    Guid UserId,
    DomainCheckSummaryResponse? LastCheck);

/// <summary>Protocol-neutral summary of a completed domain check.</summary>
/// <param name="Id">Identifier of the stored check.</param>
/// <param name="Kind">Check protocol; currently <c>http</c>.</param>
/// <param name="CheckedAt">UTC time when the check completed.</param>
/// <param name="Outcome"><c>up</c>, <c>down</c>, or <c>error</c>.</param>
public sealed record DomainCheckSummaryResponse(
    Guid Id,
    string Kind,
    DateTime CheckedAt,
    string Outcome);
