using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.ValueObjects;

namespace DomainScanner.Application.Domains.Queries.GetHttpResponse;

public class GetHttpResponseQueryHandler(IHttpScanner httpScanner) : IRequestHandler<GetHttpResponseQuery, HttpResponseObject>
{
    private readonly IHttpScanner _httpScanner =  httpScanner;
    
    public async Task<HttpResponseObject> Handle(GetHttpResponseQuery request, CancellationToken cancellationToken)
    {
        var uri = request.Domain.AddressToUri();
        if(uri is null)
            throw new UriValidationException(request.Domain.Address);

        return await _httpScanner.GetHttpResponseAsync(uri, cancellationToken);
    }
}