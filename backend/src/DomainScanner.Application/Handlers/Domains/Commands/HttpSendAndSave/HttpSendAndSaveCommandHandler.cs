using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Domains.Commands.HttpSendAndSave;

public class HttpSendAndSaveCommandHandler : IRequestHandler<HttpSendAndSaveCommand, 
    DomainCheckResult>
{
    private readonly IDomainsRepository _domainsRepository;
    private readonly IDomainCheckRepository _checkRepository;
    private readonly IUnitOfWork _uof;
    private readonly IHttpScanner _http;
    private readonly ILogger<HttpSendAndSaveCommandHandler> _logger;

    public HttpSendAndSaveCommandHandler(IDomainsRepository domainsRepository,
        IDomainCheckRepository checkRepository,
        IUnitOfWork uof,
        IHttpScanner http,
        ILogger<HttpSendAndSaveCommandHandler> logger)
    {
        _domainsRepository = domainsRepository;
        _checkRepository = checkRepository;
        _uof = uof;
        _http = http;
        _logger = logger;
    }
    
    public async Task<DomainCheckResult> Handle(HttpSendAndSaveCommand request, CancellationToken ct)
    {
        _logger.LogInformation($"Getting  domain with id {request.Id}...");
        
        // Getting domain
        var domain = await _domainsRepository.GetByIdAsync(request.Id, ct);
        if (domain is null)
        {
            _logger.LogWarning($"Domain with id {request.Id} not found");
            throw new DomainNotFoundException(request.Id);
        }
        _logger.LogInformation($"Domain with id {request.Id} was found.");
        
        // Convert address to Uri
        var uri = domain!.AddressToUri();
        if (uri is null)
        {
            _logger.LogError($"Address of domain with id {request.Id} is invalid.");
            throw new DomainUriValidationException(domain.Address);
        }
        
        // Getting http response
        _logger.LogInformation("Waiting the HTTP response...");
        var response = await _http.GetHttpResponseAsync(uri, ct);
        _logger.LogInformation($"HTTP response status code: {response.StatusCode}.");
        
        // Change the domain status
        domain.IsAvailable = response.IsSuccess;
        domain.UpdatedAt = DateTime.UtcNow;
        
        // Creating new DomainCheckResult
        var check = new DomainCheckResult()
        {
            Id = Guid.NewGuid(),
            Address = uri.ToString(),
            StatusCode = response.StatusCode,
            IsAvailable = response.IsSuccess,
            CreatedAt = DateTime.UtcNow
        };
        _logger.LogInformation($"Domain check result created: {check.Id}.");
        
        _logger.LogInformation($"Adding the domain...");
        await _checkRepository.Create(check, ct);
        _logger.LogInformation($"Domain with id {request.Id} was created.");
        
        // Adding check result and save it
        _logger.LogInformation($"Updating domain with id {request.Id}...");
        domain.CheckResults.Add(check);
        
        await _uof.SaveChangesAsync(ct);
        
        _logger.LogInformation("Operation is successful.");

        return check;
    }
}