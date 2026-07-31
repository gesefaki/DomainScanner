namespace DomainScanner.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides synchronization primitives for coordinating concurrent execution
/// in integration tests.
/// </summary>
public class ScanConcurrencyProbe
{
    private readonly SemaphoreSlim _entered = new(0);

    private readonly TaskCompletionSource _release =
        new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Signals that an operation has reached the synchronization point.
    /// </summary>
    public void NotifyEntered()
    {
        _entered.Release();
    }

    /// <summary>
    /// Waits until the specified number of operations have called
    /// <see cref="NotifyEntered"/>.
    /// </summary>
    /// <param name="expectedCount">
    /// The number of operations expected to reach the synchronization point.
    /// </param>
    /// <param name="ct">
    /// A token used to cancel the wait operation.
    /// </param>
    public async Task WaitUntilEnteredAsync(
        int expectedCount,
        CancellationToken ct
    )
    {
        for (var requestNumber = 0; requestNumber < expectedCount; requestNumber++)
        {
            await _entered.WaitAsync(ct);
        }
    }

    /// <summary>
    /// Waits until <see cref="Release"/> is called.
    /// </summary>
    /// <param name="ct">
    /// A token used to cancel the wait operation.
    /// </param>
    public Task WaitForReleaseAsync(CancellationToken ct)
    {
        return _release.Task.WaitAsync(ct);
    }

    /// <summary>
    /// Releases all operations currently waiting in
    /// <see cref="WaitForReleaseAsync"/> and allows future waiters
    /// to continue immediately.
    /// </summary>
    public void Release()
    {
        _release.TrySetResult();
    }
}