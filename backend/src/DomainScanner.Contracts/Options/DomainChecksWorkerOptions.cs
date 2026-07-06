namespace DomainScanner.Contracts.Options;

public sealed class DomainChecksWorkerOptions
{
    public const string SectionName = "DomainsChecksWorker";
    public string RecurringJobId { get; set; } = "domain-checks-recurring";
    public string CronExpression { get; set; } = "*/5 * * * *";
    public string QueueName { get; set; } = "domain-checks";
    public int BatchSize { get; set; } = 30;
}