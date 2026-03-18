namespace DomainScanner.Worker.Options;

public sealed class DomainChecksWorkerOptions
{
    public const string SectionName = "DomainsChecksWorker";

    public int IntervalSeconds { get; set; } = 300;
    public int BatchSize { get; set; } = 20;
}