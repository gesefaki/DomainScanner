using DomainScanner.Core.Models;

namespace DomainScanner.Core.Interfaces;

public interface IDomainCheckHealth
{
    public Task<bool> DomainHealthCheckAsync(Domain domain);
}