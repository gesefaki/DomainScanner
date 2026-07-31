namespace DomainScanner.Contracts.Options.RateLimiting;

public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public static class Policies
    {
        public const string Read = "read";
        public const string Write = "write";
        public const string Auth = "auth";
        public const string Login = "login";
        public const string Scan = "scan";
    }

    public SlidingWindowSettings Read { get; init; } = new();
    public SlidingWindowSettings Write { get; init; } = new();
    public SlidingWindowSettings Auth { get; init; } = new();
    public SlidingWindowSettings Login { get; init; } = new();
    public SlidingWindowSettings Scan { get; init; } = new();
    public ConcurrencySettings ScanConcurrency { get; init; } = new();

    public bool IsValid() =>
        Read.IsValid() &&
        Write.IsValid() &&
        Auth.IsValid() &&
        Login.IsValid() &&
        Scan.IsValid() &&
        ScanConcurrency.IsValid();
}

public sealed class SlidingWindowSettings
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
    public int SegmentsPerWindow { get; init; }
    public int QueueLimit { get; init; }

    public bool IsValid() =>
        PermitLimit > 0 &&
        WindowSeconds > 0 &&
        SegmentsPerWindow > 0 &&
        SegmentsPerWindow <= WindowSeconds &&
        QueueLimit >= 0;
}

public sealed class ConcurrencySettings
{
    public int PermitLimit { get; init; }
    public int QueueLimit { get; init; }

    public bool IsValid() =>
        PermitLimit > 0 &&
        QueueLimit >= 0;
}
