using DomainScanner.Contracts.DTOs.HTTPs.Responses;

namespace DomainScanner.Contracts.DTOs.Domains.Responses;

/// <summary>
/// Basic <c>DomainEntity</c> response.
/// </summary>
/// <param name="Id">The domain identifier.</param>
/// <param name="Address">The domain address.</param>
/// <param name="MonitoringEnabled">Whether scheduled monitoring is enabled; not measured availability.</param>
/// <param name="UserId">The owner's identifier.</param>
/// <param name="Checks">The retained check history included in this response.</param>
public record DomainResponse(Guid Id, 
    string Address, 
    bool? MonitoringEnabled, 
    Guid UserId,
    IEnumerable<HttpResponse> Checks);
