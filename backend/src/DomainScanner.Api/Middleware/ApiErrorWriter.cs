using DomainScanner.Contracts.Models;

namespace DomainScanner.Api.Middleware;

internal static class ApiErrorWriter
{
    public static Task WriteAsync(
        HttpContext context,
        int status,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null,
        CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(
            new ApiError(code, message, context.TraceIdentifier, errors),
            cancellationToken);
    }
}
