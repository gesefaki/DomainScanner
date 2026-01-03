using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Commands.HttpSendAndSave;

public class HttpSendAndSaveCommandHandler(IDomainsRepository domainsRepository,
    IDomainCheckRepository checkRepository,
    IUnitOfWork uof,
    IHttpScanner http)
    : IRequestHandler<HttpSendAndSaveCommand, Guid>
{
    private readonly IDomainsRepository _domainsRepository = domainsRepository;
    private readonly IDomainCheckRepository _checkRepository = checkRepository;
    private readonly IUnitOfWork _uof = uof;
    private readonly IHttpScanner _http  = http;
    
    public async Task<Guid> Handle(HttpSendAndSaveCommand request, CancellationToken cancellationToken)
    {
        // Getting domain
        var domain = request.Domain;
        _uof.Attach(domain);
        
        // Convert address to Uri
        var uri = domain!.AddressToUri();
        if (uri is null)
            throw new UriValidationError(domain.Address);

        // Getting http response
        var response = await _http.GetHttpResponseAsync(uri, cancellationToken);

        // Creating new DomainCheckResult
        var check = new DomainCheckResult()
        {
            Id = Guid.NewGuid(),
            Address = uri.ToString(),
            StatusCode = response.StatusCode,
            IsAvailable = response.IsSuccess,
            CreatedAt = DateTime.UtcNow
        };
        
        await _checkRepository.Create(check, cancellationToken);
        
        // Adding check result and save it
        domain.CheckResults.Add(check);
        
        await _uof.SaveChangesAsync(cancellationToken);
        
        return check.Id;
    }
}