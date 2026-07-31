namespace DomainScanner.Contracts.Options.Worker;

/// <summary>
/// Configuration options for the domain checks background worker. 
/// </summary>
public sealed class DomainChecksWorkerOptions
{
    /// <summary>
    /// Configuration section name for these options.
    /// </summary>
    /// <value>
    /// const <c>string</c> "DomainsChecksWorker".
    /// </value>
    public const string SectionName = "DomainsChecksWorker";

    /// <summary>
    /// Unique identifier for the recurring Hangfire job.
    /// </summary>
    /// <value>
    /// <c>string</c> identifier for the reccuring job. Default is "domain-checks-recurring".
    /// </value>
    public string RecurringJobId { get; set; } = "domain-checks-recurring";

    /// <summary>
    /// Cron expression that defines the job execution schedule.
    /// </summary>
    /// <value>
    /// A cron expression as <c>string</c>. Default is "*/5 * * * *" (every 5 minutes).
    /// </value>
    public string CronExpression { get; set; } = "*/5 * * * *";

    /// <summary>
    /// Hangfire queue name for domain check jobs.
    /// </summary>
    /// <value>
    /// A string identifying the queue. Default is "domain-checks".
    /// </value>
    public string QueueName { get; set; } = "domain-checks";

    /// <summary>
    /// Number of domains to process in each batch.
    /// </summary>
    /// <value>
    /// <c>int</c> representing the batch size. Default is 30.
    /// </value>
    public int BatchSize { get; set; } = 30;
}