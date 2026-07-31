namespace DomainScanner.Contracts.Options.RateLimiting;

/// <summary>
/// Defines rate limiting policies and their configuration settings.
/// </summary>
public sealed class RateLimitingSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Contains names of the available rate limiting policies.
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Policy for read operations.
        /// </summary>
        public const string Read = "read";

        /// <summary>
        /// Policy for write operations.
        /// </summary>
        public const string Write = "write";

        /// <summary>
        /// Policy for authentication-related operations.
        /// </summary>
        public const string Auth = "auth";

        /// <summary>
        /// Policy for login attempts.
        /// </summary>
        public const string Login = "login";

        /// <summary>
        /// Policy for domain scanning operations.
        /// </summary>
        public const string Scan = "scan";
    }

    /// <summary>
    /// Settings for read operations.
    /// </summary>
    public SlidingWindowSettings Read { get; init; } = new();

    /// <summary>
    /// Settings for write operations.
    /// </summary>
    public SlidingWindowSettings Write { get; init; } = new();

    /// <summary>
    /// Settings for authentication-related operations.
    /// </summary>
    public SlidingWindowSettings Auth { get; init; } = new();

    /// <summary>
    /// Settings for login attempts.
    /// </summary>
    public SlidingWindowSettings Login { get; init; } = new();

    /// <summary>
    /// Settings for domain scanning operations.
    /// </summary>
    public SlidingWindowSettings Scan { get; init; } = new();

    /// <summary>
    /// Concurrency settings for domain scanning operations.
    /// </summary>
    public ConcurrencySettings ScanConcurrency { get; init; } = new();

    /// <summary>
    /// Determines whether all rate limiting settings are valid.
    /// </summary>
    public bool IsValid() =>
        Read.IsValid() &&
        Write.IsValid() &&
        Auth.IsValid() &&
        Login.IsValid() &&
        Scan.IsValid() &&
        ScanConcurrency.IsValid();
}

/// <summary>
/// Defines settings for a sliding-window rate limiter.
/// </summary>
public sealed class SlidingWindowSettings
{
    /// <summary>
    /// Maximum number of permitted requests within the window.
    /// </summary>
    public int PermitLimit { get; init; }

    /// <summary>
    /// Window duration in seconds.
    /// </summary>
    public int WindowSeconds { get; init; }

    /// <summary>
    /// Number of segments within the window.
    /// </summary>
    public int SegmentsPerWindow { get; init; }

    /// <summary>
    /// Maximum number of requests allowed in the queue.
    /// </summary>
    public int QueueLimit { get; init; }

    /// <summary>
    /// Determines whether the sliding-window settings are valid.
    /// </summary>
    public bool IsValid() =>
        PermitLimit > 0 &&
        WindowSeconds > 0 &&
        SegmentsPerWindow > 0 &&
        SegmentsPerWindow <= WindowSeconds &&
        QueueLimit >= 0;
}

/// <summary>
/// Defines settings for a concurrency rate limiter.
/// </summary>
public sealed class ConcurrencySettings
{
    /// <summary>
    /// Maximum number of concurrent requests.
    /// </summary>
    public int PermitLimit { get; init; }

    /// <summary>
    /// Maximum number of requests allowed in the queue.
    /// </summary>
    public int QueueLimit { get; init; }

    /// <summary>
    /// Determines whether the concurrency settings are valid.
    /// </summary>
    public bool IsValid() =>
        PermitLimit > 0 &&
        QueueLimit >= 0;
}
