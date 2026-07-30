namespace DomainScanner.Contracts.Options;

public sealed class LoginProtectionOptions
{
    public int FailureWindowMinutes { get; set; } = 15;
    public int LockoutThreshold { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 10;
    public int MaximumLockoutMinutes { get; set; } = 60;
}