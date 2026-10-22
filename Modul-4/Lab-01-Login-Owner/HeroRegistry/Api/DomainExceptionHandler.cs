using HeroRegistry.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace HeroRegistry.Api;

public sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException)
            return false; // not ours: the default handling writes a 500

        var (status, title) = exception is InvalidStatusException
            ? (StatusCodes.Status409Conflict, "The hero's status does not allow this.")
            : (StatusCodes.Status400BadRequest, "A business rule was broken.");

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = status, Title = title, Detail = exception.Message }
        });
    }
}
