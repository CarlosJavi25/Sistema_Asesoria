using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Sanalia.Api.Middleware;

internal sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                exception.Message),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "You are not allowed to perform this operation."),
            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message),
            _ => (0, string.Empty, string.Empty)
        };

        if (status == 0)
            return false;

        logger.LogWarning(exception, "Request failed with HTTP {StatusCode}.", status);
        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        var problem = JsonSerializer.Serialize(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await httpContext.Response.WriteAsync(problem, cancellationToken);
        return true;
    }
}
