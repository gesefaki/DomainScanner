using Microsoft.AspNetCore.Antiforgery;

namespace DomainScanner.Api.Middleware;

/// <summary>
/// Provides CSRF protection for state-changing API requests.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="CsrfProtectionMiddleware"/> class.
    /// </summary>
    /// <param name="next">
    /// The next middleware in the request pipeline.
    /// </param>
    public CsrfProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Validates the antiforgery token for state-changing API requests
    /// and invokes the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">
    /// The HTTP context for the current request.
    /// </param>
    /// <param name="antiforgery">
    /// The antiforgery service used to validate the request token.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous middleware operation.
    /// </returns>
    /// <exception cref="AntiforgeryValidationException">
    /// Thrown when antiforgery validation fails.
    /// </exception>
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

    /// <summary>
    /// Determines whether the current request requires antiforgery validation.
    /// </summary>
    /// <param name="context">
    /// The HTTP context for the current request.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the request targets an API endpoint
    /// using an unsafe HTTP method; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool RequiresValidation(HttpContext context)
    {
        return
            context.Request.Path.StartsWithSegments("/api") &&
            UnsafeMethods.Contains(context.Request.Method);
    }
}
