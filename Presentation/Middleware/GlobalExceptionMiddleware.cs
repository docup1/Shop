using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Middleware;

/// <summary>
/// Глобальная обработка исключений: преобразует исключения Application-слоя в
/// RFC 7807 (application/problem+json) с соответствующим HTTP-статусом. Любые
/// неожиданные исключения маскируются в 500 без утечки внутренних деталей.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "Exception occurred after the response had already started; it cannot be translated.");
            return;
        }

        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception.");
        }
        else
        {
            logger.LogWarning(exception, "Request failed with status {Status}.", status);
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= 500 ? "An unexpected error occurred." : exception.Message,
            Instance = context.Request.Path
        }, cancellationToken: context.RequestAborted);
    }
}