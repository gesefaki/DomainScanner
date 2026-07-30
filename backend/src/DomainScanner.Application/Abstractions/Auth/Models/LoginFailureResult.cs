namespace DomainScanner.Application.Abstractions.Auth.Models;

/// <summary>
/// Represents a login attempt that ended in failure
/// </summary>
/// <param name="FailedAttempts">Number of failed attempts.</param>
/// <param name="IsBlocked">Status of access block</param>
/// <param name="Delay">Artificial delay in response until blocking</param>
/// <param name="RetryAfter">How long until the lock is released.</param>
public record LoginFailureResult(
    int FailedAttempts,
    bool IsBlocked,
    TimeSpan Delay,
    TimeSpan RetryAfter);