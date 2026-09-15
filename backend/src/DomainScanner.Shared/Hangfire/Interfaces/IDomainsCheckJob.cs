namespace DomainScanner.Shared.Hangfire.Interfaces;

/// <summary>
/// Defines the contract for a domain check job that can be scheduled with Hangfire.
/// </summary>
public interface IDomainsCheckJob
{
    /// <summary>
    /// Executes domain checks for the current job run.
    /// </summary>
    /// <param name="ct">The cancellation token for the job.</param>
    /// <returns>A task representing the asynchronous job execution.</returns>
    Task RunAsync(CancellationToken ct);
}
