namespace DomainScanner.Shared.Hangfire.Interfaces;

public interface IDomainsCheckJob
{
    Task RunAsync(CancellationToken ct);
}