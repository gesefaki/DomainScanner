using System.Text.RegularExpressions;

namespace DomainScanner.Api.Configuration;

/// <summary>
/// Transforms route parameters and controller names from PascalCase/camelCase to kebab-case format. Implements <see cref="IOutboundParameterTransformer"/>. 
/// </summary>
internal sealed class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    /// <summary>
    /// Transform the specified value to kebab-case format.
    /// </summary>
    /// <param name="value">The value to transform</param>
    /// <returns>Transformed value or <c>null</c> if input value is <c>null</c></returns>
    public string? TransformOutbound(object? value)
    {
        if (value == null)
        {
            return null;
        }

        return ToKebabCase(value.ToString()!);
    }

    /// <summary>
    /// Converts a string to kebab-case format.
    /// </summary>
    /// <param name="value">The input string to transform.</param>
    /// <returns>The kebab-case formatted string.</returns>
    private static string ToKebabCase(string value)
    {
        return Regex.Replace(
                value,
                "([a-z])([A-Z])",
                "$1-$2")
            .ToLowerInvariant();
    }
}