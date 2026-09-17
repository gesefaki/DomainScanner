using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpResponse;

/// <summary>
/// Handles <see cref="GetHttpResponseQuery"/> by verifying domain ownership
/// and executing a basic HTTP check.
/// </summary>
public class GetHttpResponseQueryHandler : IRequestHandler<GetHttpResponseQuery, HttpResponseObject>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IHttpScanner _http;

    public GetHttpResponseQueryHandler(IOwnedDomainProvider ownedDomains, IHttpScanner http)
    {
        _ownedDomains = ownedDomains;
        _http = http;
    }

    /// <inheritdoc />
    public async Task<HttpResponseObject> Handle(GetHttpResponseQuery request, CancellationToken ct)
    {
        var domain = await _ownedDomains.GetRequiredAsync(
            request.Id,
            ct);

        var uri = DomainsHelper.AddressToUri(domain)
                  ?? throw new DomainInvalidAddressFormatException(
                      domain.Address);

        return await _http.GetHttpResponseAsync(uri, ct);
    }
}
