using DomainScanner.Contracts.DTOs.HTTPs.Responses;

namespace DomainScanner.Contracts.DTOs.Domains.Responses;

/// <summary>
/// Basic <c>DomainEntity</c> response.
/// </summary>
public record DomainResponse(Guid Id, 
    string Address, 
    bool? IsAvailable, 
    Guid UserId,
    HttpResponse[] Checks);