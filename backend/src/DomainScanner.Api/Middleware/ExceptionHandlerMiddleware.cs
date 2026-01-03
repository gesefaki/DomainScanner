using DomainScanner.Api.Models;
using DomainScanner.Application.Exceptions;

namespace DomainScanner.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            BadRequestException => new ErrorResponse
            {
                StatusCode = 400,
                Message = exception.Message
            },
            UriValidationError => new ErrorResponse
            {
                StatusCode = 400,
                Message = exception.Message
            },
            DomainNotFoundException => new ErrorResponse
            {
                StatusCode = 404,
                Message = exception.Message
            },
            UserNotFoundException => new ErrorResponse
            {
                StatusCode = 404,
                Message = exception.Message
            },
            UnableToExecuteException => new ErrorResponse
            {
                StatusCode = 409,
                Message = exception.Message
            },
            _ => new ErrorResponse
            {
                StatusCode = 500,
                Message = "Internal Server Error. Please try again later."
            }
        };
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsJsonAsync(response);
    }

}