using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Services.Domains;

/// <summary>
/// Executes HTTP checks, persists their results, and updates the availability status of domains.
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

        var response = await _http.GetHttpResponseAsync(uri, ct);
        var now = DateTime.UtcNow;

        domain.IsActive = response.IsSuccess;
        domain.UpdatedAt = now;

        var check = new DomainCheckResult
        {
            Id = Guid.NewGuid(),
            Address = response.Address,
            StatusCode = response.StatusCode,
            IsActive = response.IsSuccess,
            CreatedAt = now,
            DomainId = domain.Id
        };

        await _checksRepository.CreateAsync(check, ct);

        domain.CheckResults.Add(check);
        _domainsRepository.Update(domain);

        return check;
    }
}
