using System.Globalization;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Exceptions.Users;
using Microsoft.AspNetCore.Antiforgery;
using ValidationException = FluentValidation.ValidationException;

namespace DomainScanner.Api.Middleware;

public sealed class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);

            if (context.Request.Path.StartsWithSegments("/api") &&
                !context.Response.HasStarted &&
                context.Response.StatusCode >= 400 &&
                context.Response.ContentType is null)
            {
                var (code, message) = context.Response.StatusCode switch
                {
                    400 => ("bad_request", "Bad request."),
                    401 => ("authentication_required", "Authentication required."),
                    403 => ("forbidden", "Access denied."),
                    404 => ("not_found", "Resource not found."),
                    405 => ("method_not_allowed", "Method not allowed."),
                    415 => ("unsupported_media_type", "Unsupported media type."),
                    _ => ("request_failed", "Request failed.")
                };

                await ApiErrorWriter.WriteAsync(context, context.Response.StatusCode, code, message);
            }
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted) throw;
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, code, message) = exception switch
        {
            ValidationException => (400, "validation_failed", "Request validation failed."),
            BadRequestException => (400, "bad_request", "Bad Request."),
            DomainUriValidationException => (400, "invalid_address", "Address is invalid."),
            DomainInvalidAddressFormatException => (400, "invalid_address", "Address is invalid."),
            AntiforgeryValidationException => (400, "invalid_csrf_token", "Invalid CSRF token."),
            UserInvalidCredentialsException => (401, "invalid_credentials", "Invalid email or password."),
            NonAuthenticatedException => (401, "authentication_required", "Authentication required."),
            DomainNotFoundException => (404, "domain_not_found", "Domain not found."),
            DomainCheckNotFoundException => (404, "domain_check_not_found", "Domain check not found."),
            UserNotFoundException => (404, "user_not_found", "User not found."),
            UnableToExecuteException => (409, "unable_to_execute", "Unable to execute."),
            UserConflictCredsException => (409, "user_conflict", "Username or email already exists."),
            LoginTemporarilyBlockedException => (429, "login_temporarily_blocked", "Login is temporarily blocked. Please try again later."),
            DomainQuotaExceededException => (429, "domain_quota_exceeded", "Domain quota exceeded."),
            LoginProtectionUnavailableException => (503, "service_unavailable", "Service is unavailable. Please try again later."),
            _ => (500, "internal_error", "Internal Server Error. Please try again later.")
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning("Request rejected with {StatusCode}: {ExceptionType}. TraceId: {TraceId}",
                status, exception.GetType().Name, context.TraceIdentifier);
        }

        IReadOnlyDictionary<string, string[]>? errors = exception is ValidationException validation
            ? validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray())
            : null;

        if (exception is LoginTemporarilyBlockedException blocked)
        {
            context.Response.Headers.RetryAfter = Math.Max(
                1, (int)Math.Ceiling(blocked.RetryAfter.TotalSeconds))
                .ToString(CultureInfo.InvariantCulture);
        }

        await ApiErrorWriter.WriteAsync(context, status, code, message, errors);
    }
}
