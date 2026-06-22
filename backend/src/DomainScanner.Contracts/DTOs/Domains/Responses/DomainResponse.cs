using DomainScanner.Contracts.DTOs.HTTPs.Responses;

namespace DomainScanner.Contracts.DTOs.Domains.Responses;

public record DomainResponse(Guid Id, 
    string Address, 
    bool? IsAvailable, 
    Guid UserId,
    HttpResponse[] Checks);