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
    /// The <c>DomainChecksWorker</c> section shared by JSON and environment configuration.
    /// </value>
    public const string SectionName = "DomainChecksWorker";

    /// <summary>
    /// Unique identifier for the recurring Hangfire job.
    /// </summary>
    /// <value>
    /// <c>string</c> identifier for the recurring job. Default is "domain-checks-recurring".
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
    /// Maximum number of domain identifiers in each batch processed sequentially by the worker.
    /// </summary>
    /// <value>
    /// A positive integer representing the batch size. Default is 30.
    /// </value>
    /// <remarks>
    /// Controls sequential grouping only. <see cref="MaxDomainsPerRun"/> limits the selection size.
    /// Each domain check uses a separate dependency injection scope.
    /// </remarks>
    public int BatchSize { get; set; } = 30;

    /// <summary>
    /// Maximum eligible domain identifiers selected per run, oldest updated first. Defaults to 500.
    /// </summary>
    public int MaxDomainsPerRun { get; set; } = 500;

    /// <summary>
    /// Age in days after which check results are deleted during post-run cleanup. Defaults to 30.
    /// </summary>
    public int RetentionDays { get; set; } = 30;
    
    /// <summary>
    /// Maximum newest results retained per domain after cleanup. Defaults to 100.
    /// This is a cleanup target, not a hard limit between job runs.
    /// </summary>
    public int MaxResultsPerDomain { get; set; } = 100;
}
