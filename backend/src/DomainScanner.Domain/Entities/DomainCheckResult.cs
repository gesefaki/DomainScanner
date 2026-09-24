using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

/// <summary>A persisted result of one protocol-specific check of a domain.</summary>
public class DomainCheckResult : BaseEntity
{
    /// <summary>Protocol used for the check; currently <c>http</c>.</summary>
    public string Kind { get; set; } = "http";

    /// <summary><c>up</c>, <c>down</c>, or <c>error</c>.</summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>Address configured when the check began.</summary>
    public string RequestedAddress { get; set; } = string.Empty;

    /// <summary>Final address after redirects, or null without an HTTP response.</summary>
    public string? FinalAddress { get; set; }

    /// <summary>Remote HTTP status, or null without an HTTP response.</summary>
    public int? StatusCode { get; set; }

    /// <summary>Elapsed time until response headers or failure, in milliseconds.</summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>Stable transport failure code, or null when a response was received.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Addresses followed during redirects, in order.</summary>
    public string[] Redirects { get; set; } = [];

    /// <summary>Whether TLS certificate validation reported errors, if collected.</summary>
    public bool? TlsHasValidationErrors { get; set; }

    /// <summary>UTC certificate expiration, if collected.</summary>
    public DateTime? TlsCertificateExpiresAt { get; set; }

    /// <summary>Identifier of the checked domain.</summary>
    public Guid DomainId { get; set; }

    /// <summary>Associated domain.</summary>
    public DomainEntity? DomainEntity { get; set; }
}
