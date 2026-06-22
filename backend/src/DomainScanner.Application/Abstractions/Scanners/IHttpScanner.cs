using DomainScanner.Domain.Models;

namespace DomainScanner.Application.Abstractions.Scanners;

public interface IHttpScanner
{
    Task<HttpResponseObject> GetHttpResponseAsync(Uri address, CancellationToken ct);
    Task<HttpResponseDetails> GetHttpWithDetailsAsync(Uri address, CancellationToken cancellationToken);
}