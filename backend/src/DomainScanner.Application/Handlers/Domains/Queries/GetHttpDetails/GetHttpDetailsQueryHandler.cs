using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpDetails;

/// <summary>
/// Handles <see cref="GetHttpDetailsQuery"/> by verifying domain ownership
/// and executing a detailed HTTP check.
/// </summary>
public class GetHttpDetailsQueryHandler : IRequestHandler<GetHttpDetailsQuery, HttpResponseDetails>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IHttpScanner _http;

    public GetHttpDetailsQueryHandler(IOwnedDomainProvider ownedDomains, IHttpScanner http)
    {
        _ownedDomains =  ownedDomains;
        _http = http;
    }

    /// <inheritdoc />
    public async Task<HttpResponseDetails> Handle(GetHttpDetailsQuery request, CancellationToken ct)
    {
        var domain = await _ownedDomains.GetRequiredAsync(
            request.Id,
            ct);

        var uri = DomainsHelper.AddressToUri(domain)
                  ?? throw new DomainInvalidAddressFormatException(
                      domain.Address);
        
        return await _http.GetHttpWithDetailsAsync(uri!, ct);
    }
}
