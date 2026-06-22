using DomainScanner.Contracts.DTOs.HTTPs;
using DomainScanner.Contracts.DTOs.HTTPs.Responses;

namespace DomainScanner.Contracts.DTOs.Domains.Responses;

public record HttpResponse(Guid Id, 
    string Address, 
    bool? IsAvailable, 
    Guid UserId,
    HTTPs.Responses.HttpResponse[] Checks);