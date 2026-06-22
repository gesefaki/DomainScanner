using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Helpers;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;

public class HttpSendAndSaveCommandHandler : IRequestHandler<HttpSendAndSaveCommand, DomainCheckResult>
{
    private readonly IReadRepository<DomainEntity> _domainsReadRepository;
    private readonly IWriteRepository<DomainEntity> _domainsWriteRepository;
    private readonly IWriteRepository<DomainCheckResult> _checksWriteRepository;
    private readonly IHttpScanner _http;

    public HttpSendAndSaveCommandHandler(IReadRepository<DomainEntity> domainsReadRepository, 
        IWriteRepository<DomainEntity> domainsWriteRepository,
        IWriteRepository<DomainCheckResult> checksWriteRepository, 
        IHttpScanner http)
    {
        _domainsReadRepository = domainsReadRepository;
        _domainsWriteRepository = domainsWriteRepository;
        _checksWriteRepository = checksWriteRepository;
        _http = http;
    }
    
    public async Task<DomainCheckResult> Handle(HttpSendAndSaveCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _domainsReadRepository.FindAsync(request.Id, ct);
        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }
        
        // Convert address to Uri
        var uri = DomainsHelper.AddressToUri(domain);
        if (uri is null)
        {
            throw new DomainUriValidationException(domain.Address);
        }
        
        // Getting http response
        var response = await _http.GetHttpResponseAsync(uri, ct);
        
        // Change the domain status
        domain.IsActive = response.IsSuccess;
        domain.UpdatedAt = DateTime.UtcNow;
        
        // Creating new DomainCheckResult
        var check = new DomainCheckResult()
        {
            Id = Guid.NewGuid(),
            Address = uri.ToString(),
            StatusCode = response.StatusCode,
            IsActive = response.IsSuccess,
            CreatedAt = DateTime.UtcNow
        };
        
        await _checksWriteRepository.CreateAsync(check, ct);
        
        // Adding check result and save it
        domain.CheckResults.Add(check);

        _domainsWriteRepository.Update(domain);
        
        return check;
    }
}