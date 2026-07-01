using System.Text.RegularExpressions;

internal sealed class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null)
        {
            return null;
        }

        return ToKebabCase(value.ToString()!);
    }

    private static string ToKebabCase(string value)
    {
        return Regex.Replace(
            value,
            "([a-z])([A-Z])",
            "$1-$2")
            .ToLowerInvariant();
    }
}