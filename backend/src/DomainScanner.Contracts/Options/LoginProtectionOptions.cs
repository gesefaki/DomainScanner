namespace DomainScanner.Contracts.Options;

public sealed class LoginProtectionOptions
{
    public string KeyPrefix { get; set; } = "domain-scanner";
    public int FailureWindowMinutes { get; set; } = 15;
    public int LockoutThreshold { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 10;
    public int MaximumLockoutMinutes { get; set; } = 60;
    public int EscalationWindowMinutes { get; set; } = 1440;
    public int DelayStartAttempt { get; set; } = 3;
    public int InitialDelayMilliseconds { get; set; } = 500;
    public int MaximumDelayMilliseconds { get; set; } = 2000;
}
