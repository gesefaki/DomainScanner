using DomainScanner.Domain.ValueObjects;

namespace DomainScanner.Application.Abstractions.Scanners;

public interface IHttpScanner
{
    public Task<HttpResponseObject> GetHttpResponseAsync(Uri address, CancellationToken cancellationToken);
}