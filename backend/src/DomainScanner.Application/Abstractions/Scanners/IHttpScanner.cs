using DomainScanner.Domain.Models;

namespace DomainScanner.Application.Abstractions.Scanners;

/// <summary>
/// Defines the contract for HTTP cliens implementations.
/// </summary>
public interface IHttpScanner
{
    /// <summary>
    /// Returns a base HTTP response from a specified address.
    /// </summary>
    /// <param name="address">Address converted to Uri for compatibility with <see cref="HttpClient"/>. </param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns>A task representing the async operation that returns <see cref="HttpResponseObject"/>.</returns>
    Task<HttpResponseObject> GetHttpResponseAsync(Uri address, CancellationToken ct);

    /// <summary>
    /// Returns a detailed HTTP response from a specified address.
    /// </summary>
    /// <param name="address">Address converted to Uro for compatibility with <see cref="HttpClient"/>.</param>
    /// <param name="ct">Cancellation toker provided by the user.</param>
    /// <returns>A task representing the async operation that returns <see cref="HttpResponseDetails"/>.</returns>
    Task<HttpResponseDetails> GetHttpWithDetailsAsync(Uri address, CancellationToken ct);
}