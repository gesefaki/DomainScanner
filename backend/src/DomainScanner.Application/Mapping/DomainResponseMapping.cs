using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.HTTPs.Responses;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Mapping;

/// <summary>Single source of mappings for stored domains and checks.</summary>
public static class DomainResponseMapping
{
    /// <summary>Maps domain metadata with only its latest check summary.</summary>
    public static DomainResponse ToDomain(DomainEntity domain)
    {
        var latest = domain.CheckResults
            .OrderByDescending(check => check.CreatedAt)
            .ThenByDescending(check => check.Id)
            .FirstOrDefault();

        return new DomainResponse(
            domain.Id,
            domain.Address,
            domain.MonitoringEnabled,
            domain.UserId,
            latest is null ? null : ToSummary(latest));
    }

    /// <summary>Maps a stored check without inventing HTTP data for failed connections.</summary>
    public static DomainCheckResponse ToCheck(DomainCheckResult check)
    {
        var http = check.Kind == "http"
            ? new HttpCheckResponse(
                check.RequestedAddress,
                check.FinalAddress,
                check.StatusCode,
                check.ResponseTimeMs,
                check.ErrorCode,
                check.Redirects,
                check.TlsHasValidationErrors is null &&
                check.TlsCertificateExpiresAt is null
                    ? null
                    : new HttpTlsResponse(
                        check.TlsHasValidationErrors,
                        check.TlsCertificateExpiresAt))
            : null;

        return new DomainCheckResponse(
            check.Id,
            check.DomainId,
            check.Kind,
            check.CreatedAt,
            check.Outcome,
            http);
    }

    /// <summary>Maps fields shared by future check protocols.</summary>
    public static DomainCheckSummaryResponse ToSummary(DomainCheckResult check) =>
        new(check.Id, check.Kind, check.CreatedAt, check.Outcome);
}
