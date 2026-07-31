namespace DomainScanner.Contracts.Options.Login;

/// <summary>
/// Defines settings for failed login tracking and temporary lockouts.
/// </summary>
public sealed class LoginProtectionOptions
{
    /// <summary>
    /// Prefix applied to login protection keys stored in Redis.
    /// </summary>
    public string KeyPrefix { get; set; } = "domain-scanner";

    /// <summary>
    /// Duration in minutes during which failed attempts are counted.
    /// </summary>
    public int FailureWindowMinutes { get; set; } = 15;

    /// <summary>
    /// Number of failed attempts required to block login.
    /// </summary>
    public int LockoutThreshold { get; set; } = 5;

    /// <summary>
    /// Initial login lockout duration in minutes.
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 10;

    /// <summary>
    /// Maximum login lockout duration in minutes.
    /// </summary>
    public int MaximumLockoutMinutes { get; set; } = 60;

    /// <summary>
    /// Duration in minutes during which repeated lockouts are escalated.
    /// </summary>
    public int EscalationWindowMinutes { get; set; } = 1440;

    /// <summary>
    /// Failed attempt number from which an artificial delay is applied.
    /// </summary>
    public int DelayStartAttempt { get; set; } = 3;

    /// <summary>
    /// Initial artificial delay in milliseconds.
    /// </summary>
    public int InitialDelayMilliseconds { get; set; } = 500;

    /// <summary>
    /// Maximum artificial delay in milliseconds.
    /// </summary>
    public int MaximumDelayMilliseconds { get; set; } = 2000;
}
