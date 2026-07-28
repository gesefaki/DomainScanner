using Microsoft.AspNetCore.Antiforgery;

namespace DomainScanner.Api.Middleware;

public sealed class CsrfProtectionMiddleware
{
    private static readonly HashSet<string> UnsafeMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Post,
            HttpMethods.Put,
            HttpMethods.Patch,
            HttpMethods.Delete
        };

    private readonly RequestDelegate _next;

    public CsrfProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAntiforgery antiforgery)
    {
        if (RequiresValidation(context))
        {
            await antiforgery.ValidateRequestAsync(context);
        }

        await _next(context);
    }

    private static bool RequiresValidation(HttpContext context)
    {
        return
            context.Request.Path.StartsWithSegments("/api") &&
            UnsafeMethods.Contains(context.Request.Method);
    }
}
