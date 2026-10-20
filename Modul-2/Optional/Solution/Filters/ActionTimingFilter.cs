using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HeroRegistry.Filters;

// Runs around every action it is applied to: after binding and validation,
// before and after the action itself.
public sealed class ActionTimingFilter(ILogger<ActionTimingFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var start = Stopwatch.GetTimestamp();

        await next();

        logger.LogInformation("{Action} took {ElapsedMs:0.0} ms, ModelState valid: {IsValid}",
            context.ActionDescriptor.DisplayName,
            Stopwatch.GetElapsedTime(start).TotalMilliseconds,
            context.ModelState.IsValid);
    }
}
