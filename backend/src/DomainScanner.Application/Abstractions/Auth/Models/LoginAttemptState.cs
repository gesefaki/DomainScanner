namespace DomainScanner.Application.Abstractions.Auth.Models;

/// <summary>
/// Represents a login attempt.
/// </summary>
/// <param name="IsBlocked">Status of access block.</param>
/// <param name="FailedAttempts">Number of failed attempts.</param>
/// <param name="RetryAfter">How long until the lock is released.</param>
public record LoginAttemptState(
    bool IsBlocked,
    int FailedAttempts,
    TimeSpan RetryAfter);