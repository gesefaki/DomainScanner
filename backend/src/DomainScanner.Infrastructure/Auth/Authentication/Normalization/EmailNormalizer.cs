using System.Text;
using DomainScanner.Application.Abstractions.Auth;

namespace DomainScanner.Infrastructure.Auth.Authentication.Normalization;

/// <summary>
/// Implements the <see cref="IEmailNormalizer"/> contract for the general email normalization.
/// </summary>
public sealed class EmailNormalizer : IEmailNormalizer
{
    /// <inheritdoc/>
    public string Normalize(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        return email
            .Trim()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }
}