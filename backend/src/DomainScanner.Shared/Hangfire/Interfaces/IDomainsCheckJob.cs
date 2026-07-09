namespace DomainScanner.Shared.Hangfire.Interfaces;

/// <summary>
/// Defines the contract for a domain check job that can be scheduled with Hangfire.
/// </summary>
public interface IDomainsCheckJob
{
    Task RunAsync(CancellationToken ct);
}