# Lab 02 — Who may do what

**Time:** 10:29–11:03 (30 min of work, then 4 min review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` project in this folder: the Lab 01 solution plus the instructor's Demo 2, the policy `CanRetire` (`RequireRole("Admin")`) on `POST api/heroes/{id}/retire`.

```bash
cd Modul-4/Lab-02-Authorization/HeroRegistry
```

The database `HeroRegistry` is the same as in Lab 01 (no new migration). If you created the `AddHeroOwner` migration in Lab 01, don't do anything with the database. If you didn't get that far: `Update-Database`. Create your tokens again if this is a fresh clone (`dotnet user-jwts create --name alice --role Admin`, `... --name bob`) and paste them into `HeroRegistry.http`.

## Goal

`[Authorize]` answers "is the caller signed in?" and a role policy answers "is the caller an Admin?". Neither can answer "is the caller the owner of **this** hero?", because the hero is loaded inside the action. That is resource-based authorization: a requirement, a handler that gets the user and the hero, and `IAuthorizationService` in the action. Order in the action: authenticate → load → authorize the resource → domain rule.

## Plan

- **First 3 min**: run, send the two Demo 2 requests (bob retires → 403, alice retires → 200).
- **Next 10 min**: task 1 (requirement, handler, policy).
- **Next 7 min**: task 2 (API delete).
- **Next 8 min**: task 3 (delete from My drafts).
- **Next 2 min**: the check.
- **Last 4 min**: review with the instructor.

## Task 1 — the owner requirement

1. `Security/HeroOwnerRequirement.cs`: `public sealed class HeroOwnerRequirement : IAuthorizationRequirement;` — a marker, nothing inside.
2. `Security/HeroOwnerHandler.cs`: `AuthorizationHandler<HeroOwnerRequirement, Hero>`. In `HandleRequirementAsync(context, requirement, hero)`: if `context.User.Identity?.Name == hero.OwnerName`, call `context.Succeed(requirement)`. Return `Task.CompletedTask`. Not calling `Succeed` means "no".
3. `Program.cs`, inside `AddAuthorization(options => ...)`: `options.AddPolicy("HeroOwner", policy => policy.AddRequirements(new HeroOwnerRequirement()));` and after it `builder.Services.AddSingleton<IAuthorizationHandler, HeroOwnerHandler>();` (`using Microsoft.AspNetCore.Authorization;`).

<details>
<summary>Hint (open only if you're stuck)</summary>

`Security/HeroOwnerHandler.cs`:

```csharp
using HeroRegistry.Models;
using Microsoft.AspNetCore.Authorization;

namespace HeroRegistry.Security;

public sealed class HeroOwnerHandler : AuthorizationHandler<HeroOwnerRequirement, Hero>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HeroOwnerRequirement requirement,
        Hero hero)
    {
        if (context.User.Identity?.Name == hero.OwnerName)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

`Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanRetire", policy => policy.RequireRole("Admin"));
    options.AddPolicy("HeroOwner", policy => policy.AddRequirements(new HeroOwnerRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, HeroOwnerHandler>();
```

</details>

## Task 2 — only the owner deletes through the API

`HeroesApiController`: add `IAuthorizationService authorization` to the primary constructor. In `Delete`, after the hero is found and before `EnsureCanBeDeleted()`:

```csharp
var decision = await authorization.AuthorizeAsync(User, hero, "HeroOwner");
if (!decision.Succeeded)
    return Forbid();
```

`Forbid()` is 403 for the Bearer scheme. Keep `[Authorize]` on the action: without it an anonymous caller would reach the handler and get 403 instead of 401.

## Task 3 — Delete from My drafts

1. `Views/Heroes/Drafts.cshtml`: next to each codename a form with one button: `<form asp-action="Delete" asp-route-id="@hero.Id" method="post"><button type="submit">Delete</button></form>`.
2. `HeroesController`: add `IAuthorizationService authorization` to the constructor and a `Delete(int id)` action: `[Authorize]`, `[HttpPost]`, `[ValidateAntiForgeryToken]`, `async Task<IActionResult>`. Find the hero (`NotFound()` if missing), the same `AuthorizeAsync` check (`Forbid()`), then `EnsureCanBeDeleted()`, `Remove`, `SaveChanges`, a `TempData` message, redirect to `Drafts`.

The page shows only my drafts, so why check again? Because a POST can carry any id. The UI hides, the server enforces.

<details>
<summary>Hint (open only if you're stuck)</summary>

`HeroesController`, the constructor and the new action:

```csharp
public sealed class HeroesController(IHeroStore store, IAuthorizationService authorization) : Controller
```

```csharp
[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var hero = store.Heroes.FirstOrDefault(h => h.Id == id);
    if (hero is null)
        return NotFound();

    var decision = await authorization.AuthorizeAsync(User, hero, "HeroOwner");
    if (!decision.Succeeded)
        return Forbid(); // cookie scheme: redirect to /Account/Denied

    hero.EnsureCanBeDeleted();
    store.Remove(hero);
    store.SaveChanges();

    TempData["Message"] = $"Draft '{hero.Codename}' deleted.";
    return RedirectToAction(nameof(Drafts));
}
```

`Views/Heroes/Drafts.cshtml`, the list item:

```html
<li>
    @hero.Codename
    <form asp-action="Delete" asp-route-id="@hero.Id" method="post">
        <button type="submit">Delete</button>
    </form>
</li>
```

</details>

## Check

`HeroRegistry.http`, section "Lab 2: who may do what", from top to bottom. Then in the browser: log in as bob, create a Draft; log in as alice in a private window; from alice's window send bob's Delete form (change the id in the browser's dev tools, or POST with the `.http` file) → `/Account/Denied`.

## Done when...

- Bob deleting alice's Draft → **403**; alice deleting it → **204**.
- Alice deleting her own Registered hero ("Veteran" in `HeroRegistry.http`) → **409** — authorization passed, the domain rule said no.
- Anonymous `DELETE` → **401**.
- Bob retiring → **403**, alice (Admin) → **200**.
- The Delete button on `/Heroes/Drafts` removes my Draft and shows a message; a POST for someone else's hero ends on `/Account/Denied`.

## Help, if you get stuck

- **`InvalidOperationException: No authorization handler for requirement HeroOwnerRequirement`** — the handler is not registered (`AddSingleton<IAuthorizationHandler, HeroOwnerHandler>()`), or the policy name differs.
- **The owner gets 403 too**: the handler compares `hero.OwnerName` with `context.User.Identity?.Name`; a hero from Day 3 has an empty owner, so nobody owns it. Use a hero created today.
- **403 instead of 401 for anonymous**: `[Authorize]` is missing on the action.
- **`Update-Database` says `Column names in each table must be unique ... 'OwnerName'`**: the column already exists from your own Lab 01 migration (same change, different name). Skip `Update-Database`, or for a clean start run `Drop-Database`, then `Update-Database`.
- **The Delete button returns 404**: the form has no `method="post"`, so it sends a GET, and the action is `[HttpPost]`.
- **MVC: the handler never runs**: `AuthorizeAsync(User, hero, "HeroOwner")` — the second argument must be the `Hero`, not its id.

## Bonus — optional, only if time allows

1. **Admin deletes any Draft.** In `HeroOwnerHandler`, also succeed when `context.User.IsInRole("Admin")`. Check: alice (Admin) deletes bob's Draft → 204.
2. **A claim instead of a role.** Add a claim `"department", "registry"` to alice in `AccountController` and in the token (`dotnet user-jwts create --name carol --claim department=registry`). A policy `CanReview` with `RequireClaim("department", "registry")` on a new `GET api/heroes/drafts` (all drafts, any owner).
3. **Retire in the Minimal API**, with the same policy: `.RequireAuthorization("CanRetire")`.

**Important:** the reference solution will be published in this folder as `Solution/` after the lab. Lab 03 starts from the reference version, no matter how far you got.
