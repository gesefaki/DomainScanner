using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpResponse;

/// <summary>
/// Handles <see cref="GetHttpResponseQuery"/>
/// </summary>
public class GetHttpResponseQueryHandler : IRequestHandler<GetHttpResponseQuery, HttpResponseObject>
{
    private readonly IReadRepository<DomainEntity, Guid> _repository;
    private readonly IHttpScanner _http;

    public GetHttpResponseQueryHandler(IReadRepository<DomainEntity, Guid> repository, IHttpScanner http)
    {
        _http = http;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<HttpResponseObject> Handle(GetHttpResponseQuery request, CancellationToken ct)
    {
        var domain = await _repository.FindAsync(request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }
        
        var uri = DomainsHelper.AddressToUri(domain);

        return await _http.GetHttpResponseAsync(uri!, ct);
    }
}