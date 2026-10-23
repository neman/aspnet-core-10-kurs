using HeroRegistry.Models;
using Microsoft.AspNetCore.Authorization;

namespace HeroRegistry.Security;

// [Authorize] alone cannot do this, because the hero is loaded inside the action.
public sealed class HeroOwnerHandler : AuthorizationHandler<HeroOwnerRequirement, Hero>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HeroOwnerRequirement requirement,
        Hero hero)
    {
        if (context.User.Identity?.Name == hero.OwnerName)
            context.Succeed(requirement);

        // Not calling Succeed means "not satisfied": the caller gets 403.
        return Task.CompletedTask;
    }
}
