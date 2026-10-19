using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CourseApi.Middleware;

// SOLUTION Task A: logs the path before next() and the status code after it.
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Before {Path}", context.Request.Path);
        await _next(context);
        _logger.LogInformation("After {StatusCode}", context.Response.StatusCode);
    }
}
