using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Services.Domains;

/// <summary>
/// Executes HTTP checks, persists transport-aware results, and updates measured domain availability.
/// </summary>
public sealed class DomainCheckExecutor : IDomainCheckExecutor
{
    private readonly IRepository<DomainEntity, Guid> _domainsRepository;
    private readonly IWriteRepository<DomainCheckResult, Guid> _checksRepository;
    private readonly IHttpScanner _http;

    public DomainCheckExecutor(
        IRepository<DomainEntity, Guid> domainsRepository,
        IWriteRepository<DomainCheckResult, Guid> checksRepository,
        IHttpScanner http)
    {
        _domainsRepository = domainsRepository;
        _checksRepository = checksRepository;
        _http = http;
    }

    /// <inheritdoc />
    /// <exception cref="DomainInvalidAddressFormatException">
    /// Thrown when the stored domain address cannot be converted to a valid URI.
    /// </exception>
    public async Task<DomainCheckResult> ExecuteAndSaveAsync(
        DomainEntity domain,
        CancellationToken ct
    )
    {
        var uri = DomainsHelper.AddressToUri(domain)
                  ?? throw new DomainInvalidAddressFormatException(
                      domain.Address);

        var response = await _http.CheckAsync(uri, ct);
        var now = DateTime.UtcNow;
        var outcome = response.ErrorCode is not null
            ? "error"
            : response.IsSuccess ? "up" : "down";

        domain.IsActive = response.IsSuccess;
        domain.UpdatedAt = now;

        var check = new DomainCheckResult
        {
            Id = Guid.NewGuid(),
            Kind = "http",
            Outcome = outcome,
            RequestedAddress = response.RequestedAddress,
            FinalAddress = response.FinalAddress,
            StatusCode = response.StatusCode,
            ResponseTimeMs = response.ResponseTimeMs,
            ErrorCode = response.ErrorCode,
            Redirects = response.Redirects.ToArray(),
            TlsHasValidationErrors = response.Tls?.SslPolicyErrors,
            TlsCertificateExpiresAt = response.Tls?.CertificateExpiresAt,
            IsActive = outcome == "up",
            CreatedAt = now,
            DomainId = domain.Id
        };

        await _checksRepository.CreateAsync(check, ct);

        domain.CheckResults.Add(check);
        _domainsRepository.Update(domain);

        return check;
    }
}
