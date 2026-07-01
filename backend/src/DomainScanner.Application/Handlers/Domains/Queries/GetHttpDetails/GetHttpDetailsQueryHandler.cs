using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpDetails;

public class GetHttpDetailsQueryHandler : IRequestHandler<GetHttpDetailsQuery, HttpResponseDetails>
{
    private readonly IReadRepository<DomainEntity, Guid> _repository;
    private readonly IHttpScanner _http;

    public GetHttpDetailsQueryHandler(IReadRepository<DomainEntity, Guid> repository, IHttpScanner http)
    {
        _repository = repository;
        _http = http;
    }
    
    public async Task<HttpResponseDetails> Handle(GetHttpDetailsQuery request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _repository.FindAsync(request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }
        
        // Address to Uri
        var uri = DomainsHelper.AddressToUri(domain);
        if (uri is null)
        {
            throw new DomainUriValidationException(domain.Address);
        }
        
        return await _http.GetHttpWithDetailsAsync(uri, ct);
    }
}