using DomainScanner.Api.Models;
using DomainScanner.Application.Exceptions;

namespace DomainScanner.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            BadRequestException => new ErrorResponse
            {
                StatusCode = 400,
                Message = "Bad Request."
            },
            UriValidationException => new ErrorResponse
            {
                StatusCode = 400,
                Message = "Address is invalid."
            },
            InvalidAddressFormatException => new ErrorResponse()
            {
                StatusCode = 400,
                Message = "Address is invalid."
            },
            InvalidCredentialsException => new ErrorResponse()
            {
                StatusCode = 401,
                Message = "Invalid email or password."
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
            ConflictCredsException => new ErrorResponse
            {
                StatusCode = 409,
                Message = "Username or email already exists."
            },
            _ => new ErrorResponse
            {
                StatusCode = 500,
                Message = "Internal Server Error. Please try again later."
            }
        };
        _logger.LogError(exception.Message);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);

    }

}