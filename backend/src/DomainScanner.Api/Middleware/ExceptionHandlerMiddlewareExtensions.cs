namespace DomainScanner.Api.Middleware;

/// <summary>
/// Provides extension methods for registering the exception handler middleware.
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handler middleware to the application pipeline.
    /// </summary>
    /// <param name="app"></param>
    public static void UseExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}