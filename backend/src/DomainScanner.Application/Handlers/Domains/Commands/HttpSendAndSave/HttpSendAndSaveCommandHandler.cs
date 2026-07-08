using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Helpers;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;

/// <summary>
/// Handles <see cref="HttpSendAndSaveCommand"/>. 
/// </summary>
public class HttpSendAndSaveCommandHandler : IRequestHandler<HttpSendAndSaveCommand, DomainCheckResult>
{
    private readonly IRepository<DomainEntity, Guid> _domainsRepository;
    private readonly IWriteRepository<DomainCheckResult, Guid> _checksWriteRepository;
    private readonly IHttpScanner _http;

    public HttpSendAndSaveCommandHandler(IRepository<DomainEntity, Guid> domainsRepository,
        IWriteRepository<DomainCheckResult, Guid> checksWriteRepository, 
        IHttpScanner http)
    {
        _domainsRepository = domainsRepository;
        _checksWriteRepository = checksWriteRepository;
        _http = http;
    }
    
    /// <inheritdoc />
    public async Task<DomainCheckResult> Handle(HttpSendAndSaveCommand request, CancellationToken ct)
    {
        // Getting domain
        var domain = await _domainsRepository.FindAsync(request.Id, ct);
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

        _domainsRepository.Update(domain);
        
        return check;
    }
}