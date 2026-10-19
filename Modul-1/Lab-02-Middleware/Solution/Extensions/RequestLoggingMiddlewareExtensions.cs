using CourseApi.Middleware;
using Microsoft.AspNetCore.Builder;

namespace CourseApi.Extensions;

// SOLUTION Task A: registration extension used in Program.cs.
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
