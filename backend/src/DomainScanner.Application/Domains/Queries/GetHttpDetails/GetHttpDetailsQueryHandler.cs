using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Queries.GetHttpDetails;

public class GetHttpDetailsQueryHandler : IRequestHandler<GetHttpDetailsQuery, HttpResponseDetails>
{
    private readonly IDomainsRepository _domainsRepository;
    private readonly IHttpScanner _scanner;
    private readonly ILogger<GetHttpDetailsQueryHandler> _logger;

    public GetHttpDetailsQueryHandler(IDomainsRepository domainsRepository,
        IHttpScanner scanner,
        ILogger<GetHttpDetailsQueryHandler> logger)
    {
        _domainsRepository = domainsRepository;
        _scanner = scanner;
        _logger = logger;
    }
    
    public async Task<HttpResponseDetails> Handle(GetHttpDetailsQuery request, CancellationToken ct)
    {
        // Getting domain
        _logger.LogInformation($"Getting domain with id {request.Id}...");
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        if (domain is null)
        {
            _logger.LogWarning($"Domain with id {request.Id} not found.");
            throw new DomainNotFoundException( request.Id);
        }

        _logger.LogInformation($"Domain with id {request.Id} was found.");

        // Address to Uri
        var uri = domain.AddressToUri();
        if (uri is null)
        {
            _logger.LogError($"Address of domain  with id {request.Id} is invalid.");
            throw new UriValidationException(domain.Address);
        }

        _logger.LogInformation("Operation successful.");
        
        return await _scanner.GetHttpWithDetailsAsync(uri, ct);
    }
}