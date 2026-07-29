using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.IntegrationTests.Controllers;


/// <summary>
/// Provides test-only endpoints for verifying rate limiting behavior
/// in integration tests.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("__tests/rate-limiting")]
public sealed class RateLimitingProbeController : ControllerBase
{
    private readonly ScanConcurrencyProbe _concurrencyProbe;

    public RateLimitingProbeController(ScanConcurrencyProbe concurrencyProbe)
    {
        _concurrencyProbe = concurrencyProbe;
    }

    /// <summary>
    /// Provides an endpoint configured with the write rate limiting policy.
    /// </summary>
    /// <returns>
    /// A <see cref="StatusCodes.Status204NoContent"/> response
    /// when the request is allowed.
    /// </returns>
    [HttpGet("write")]
    [EnableRateLimiting(RateLimitingOptions.Write)]
    public IActionResult Write() => NoContent();

    
    /// <summary>
    /// Provides an endpoint configured with the scan rate limiting policy.
    /// </summary>
    /// <returns>
    /// A <see cref="StatusCodes.Status204NoContent"/> response
    /// when the request is allowed.
    /// </returns>
    [HttpGet("scan")]
    [EnableRateLimiting(RateLimitingOptions.Scan)]
    public IActionResult Scan() => NoContent();

    /// <summary>
    /// Blocks the request until released by the test.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the wait.
    /// </param>
    /// <returns>
    /// A <see cref="StatusCodes.Status204NoContent"/> response.
    /// </returns>
    [HttpGet("scan-concurrency")]
    [EnableRateLimiting(RateLimitingOptions.Scan)]
    public async Task<IActionResult> ScanConcurrency(CancellationToken cancellationToken)
    {
        _concurrencyProbe.NotifyEntered();

        await _concurrencyProbe.WaitForReleaseAsync(cancellationToken);

        return NoContent();
    }
}