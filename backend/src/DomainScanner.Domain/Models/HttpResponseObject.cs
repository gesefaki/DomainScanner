namespace DomainScanner.Domain.Models;

/// <summary>
/// A base model that stores the status of an HTTP response from a specific address. 
/// </summary>
public class HttpResponseObject
{
    /// <summary>
    /// Requests address. Supports any format supported by HTTP.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The response code returned by the address.
    /// </summary>
    public ushort StatusCode { get; set; }

    /// <summary>
    /// Indicates whether the address is accessible.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// The entity's creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}