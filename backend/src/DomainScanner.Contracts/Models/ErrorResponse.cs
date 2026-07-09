namespace DomainScanner.Contracts.Models;

/// <summary>
/// Standarized error response retured to API clients.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP status code returned in the response.
    /// </summary>
    /// <value>
    /// <c>int</c> which represents HTTP status code (e.g 200, 400, 404).
    /// </value>
    public int StatusCode { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    /// <value>
    /// Error message from HTTP response as a <c>string</c>.
    /// </value>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the error was created.
    /// </summary>
    /// <value> 
    /// Defaults to <see cref="DateTime.UtcNow"/>
    /// </value>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}