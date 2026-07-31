using DomainScanner.Contracts.Exceptions.Common;
using System.Globalization;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Contracts.Models;
using Microsoft.AspNetCore.Antiforgery;

namespace DomainScanner.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP request and handle any exceptions.
    /// </summary>
    /// <param name="context">HTTP context.</param>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles the exception by mapping it to an appropriate HTTP response.
    /// </summary>
    /// <param name="context">HTTP context.</param>
    /// <param name="exception">The exception that was thrown.</param>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            BadRequestException => new ErrorResponse
            {
                StatusCode = 400,
                Message = "Bad Request."
            },
            DomainUriValidationException => new ErrorResponse
            {
                StatusCode = 400,
                Message = "Address is invalid."
            },
            DomainInvalidAddressFormatException => new ErrorResponse()
            {
                StatusCode = 400,
                Message = "Address is invalid."
            },
            AntiforgeryValidationException => new ErrorResponse()
            {
                StatusCode = 400,
                Message = "Invalid CSRF token."
            },
            UserInvalidCredentialsException => new ErrorResponse()
            {
                StatusCode = 401,
                Message = "Invalid email or password."
            },
            NonAuthenticatedException => new ErrorResponse()
            {
                StatusCode = 401,
                Message = "Authentication required."
            },
            DomainNotFoundException => new ErrorResponse
            {
                StatusCode = 404,
                Message = "Domain not found."
            },
            UserNotFoundException => new ErrorResponse
            {
                StatusCode = 404,
                Message = "User not found."
            },
            UnableToExecuteException => new ErrorResponse
            {
                StatusCode = 409,
                Message = "Unable to execute."
            },
            UserConflictCredsException => new ErrorResponse
            {
                StatusCode = 409,
                Message = "Username or email already exists."
            },
            LoginTemporarilyBlockedException => new ErrorResponse
            {
                StatusCode = StatusCodes.Status429TooManyRequests,
                Message = "Login is temporarily blocked. Please try again later."
            },
            LoginProtectionUnavailableException => new ErrorResponse()
            {
                StatusCode = 503,
                Message = "Service is unavailable. Please try again later."
            },
            _ => new ErrorResponse
            {
                StatusCode = 500,
                Message = "Internal Server Error. Please try again later."
            }
        };

        if (exception is LoginTemporarilyBlockedException blocked)
        {
            context.Response.Headers.RetryAfter = Math.Max(
                    1,
                    (int)Math.Ceiling(blocked.RetryAfter.TotalSeconds))
                .ToString(CultureInfo.InvariantCulture);
        }

        if (response.StatusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Request rejected with {StatusCode}: {ExceptionType}. " +
                "TraceId: {TraceId}",
                response.StatusCode,
                exception.GetType().Name,
                context.TraceIdentifier);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);

    }

}
