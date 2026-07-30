using DomainScanner.Application.Abstractions.Auth.Models;

namespace DomainScanner.Application.Abstractions.Auth;

/// <summary>
/// Defines the contract that limits the number of login attempts for a specific user.
/// </summary>
public interface ILoginAttemptProtector
{
    /// <summary>
    /// Retrieving the login status.
    /// </summary>
    /// <param name="accountKey">The account key used for logging in.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Login attempt state.</returns>
    Task<LoginAttemptState> GetStateAsync(
        string accountKey,
        CancellationToken ct
    );

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    /// <param name="accountKey">The account key used for logging in.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Login failure result.</returns>
    Task<LoginFailureResult> RegisterFailureAsync(
        string accountKey,
        CancellationToken ct
        );

    /// <summary>
    /// Allows to try logging in again.
    /// </summary>
    /// <param name="accountKey">The account key used for logging in.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ResetAsync(
        string accountKey,
        CancellationToken ct
    );
}