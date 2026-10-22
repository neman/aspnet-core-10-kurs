# Lab 01 — Sign-in and the hero's owner

**Time:** 09:22–10:05 (35 min of work, then 5 min review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` project in this folder. It is the Day 3 reference version plus the instructor's Demo 1: two authentication schemes in `Program.cs` (a cookie for the browser, a JWT bearer token for the API), `AccountController` with Login/Logout, course users in `appsettings.json`, and `[Authorize]` on the two Create actions.

```bash
cd Modul-4/Lab-01-Login-Owner/HeroRegistry
```

Before you start:

1. Bring the database up to date: `Update-Database` (Package Manager Console, *Default project*: `HeroRegistry`) or `dotnet ef database update`. The database `HeroRegistry` from Day 3 is reused. If it is already up to date, you get "No migrations were applied"; if you don't have it yet, it is created with the seed heroes.
2. Create two development tokens for the API, once, in the project folder:

   ```bash
   dotnet user-jwts create --name alice --role Admin
   dotnet user-jwts create --name bob
   ```

   Each command prints a token. Paste them into `HeroRegistry.http` as `@aliceToken` and `@bobToken`. The signing key is stored in your user secrets, so the tokens are valid only on your machine (for three months).

Then run the project with **F5** or `dotnet run` (`http://localhost:5100`). Course users for the browser: `alice` / `alice1` (role Admin) and `bob` / `bob1`.

## Goal

Every hero has an owner: the user who created it. The owner comes from the signed-in user (the cookie or the token), never from the form or the JSON. Every write needs a signed-in user; reads stay public. Each user sees their own Drafts on `/Heroes/Drafts`.

## Plan

- **First 5 min**: tokens, run, try `GET /Heroes/Create` anonymously (login page) and the first three requests in `HeroRegistry.http` (401 without a token, 201 with one).
- **Next 10 min**: task 1 (owner in the domain, migration).
- **Next 8 min**: task 2 (owner from the user).
- **Next 5 min**: task 3 (protect every write).
- **Next 7 min**: task 4 (My drafts).
- **Last 5 min**: review with the instructor.

## Task 1 — the owner in the domain

1. `Models/Hero.cs`: a property `public string OwnerName { get; private set; } = null!;` (`null!` only tells the compiler that EF Core or the constructor always sets it; there is no default owner) and a new **first** constructor parameter `string ownerName`. An empty owner throws `BusinessRuleException("A hero needs an owner.")`.
2. `Models/HeroQueries.cs`: an extension `OwnedBy(this IQueryable<Hero> heroes, string ownerName)`.
3. `Data/HeroDbContext.cs`: `hero.Property(h => h.OwnerName).HasMaxLength(60);`
4. `Data/HeroSeed.cs`: the three seed heroes belong to `"alice"`.
5. The build now fails in three places (MVC Create, API Create, Minimal API Create): pass `"alice"` for the moment, so you can run the migration. Task 2 replaces it.
6. `Add-Migration AddHeroOwner`, then `Update-Database`. Existing heroes get an empty `OwnerName`.

<details>
<summary>Hint (open only if you're stuck)</summary>

`Models/Hero.cs`, the constructor and the new property:

```csharp
public Hero(
    string ownerName,
    string codename,
    string originStory,
    HeroRace race,
    HeroAlignment alignment)
{
    if (string.IsNullOrWhiteSpace(ownerName))
        throw new BusinessRuleException("A hero needs an owner.");

    OwnerName = ownerName;
    SetDetails(
        codename,
        originStory,
        race,
        alignment);
    Status = HeroStatus.Draft;
}

public string OwnerName { get; private set; } = null!;
```

`Models/HeroQueries.cs`:

```csharp
public static IQueryable<Hero> OwnedBy(this IQueryable<Hero> heroes, string ownerName) =>
    heroes.Where(h => h.OwnerName == ownerName);
```

`Data/HeroSeed.cs`: `"alice"` as the first argument of each `new Hero(...)`.

</details>

## Task 2 — the owner is the signed-in user

The name of the signed-in user is `User.Identity!.Name!` in a controller (the `Name` claim; `[Authorize]` guarantees it is there).

1. `HeroesController.Create` (POST) and `HeroesApiController.Create`: pass `User.Identity!.Name!` instead of `"alice"`.
2. `Api/HeroEndpoints.cs`, the `MapPost("/")` endpoint: add a parameter `ClaimsPrincipal user` to the lambda (`using System.Security.Claims;`) and pass `user.Identity!.Name!`. Then protect the endpoint: `.RequireAuthorization()` after `.AddEndpointFilter<...>()`. That is the Minimal API form of `[Authorize]`.
3. `Api/HeroContracts.cs`: add `string Owner` to `HeroResponse` (after `Id`) and map it from `hero.OwnerName`, so a client sees who owns a hero.

<details>
<summary>Hint (open only if you're stuck)</summary>

`HeroesController.Create` (POST); the API controller is the same with `request` instead of `input`:

```csharp
// The owner comes from the cookie, never from the form.
store.Add(new Hero(
    User.Identity!.Name!,
    input.Codename,
    input.OriginStory ?? "",
    input.Race!.Value,
    input.Alignment!.Value));
```

`Api/HeroEndpoints.cs`:

```csharp
heroes.MapPost("/", async (
    CreateHeroRequest request,
    ClaimsPrincipal user,
    IHeroStore store) =>
{
    var hero = new Hero(
        user.Identity!.Name!,
        request.Codename,
        request.OriginStory ?? "",
        request.Race!.Value,
        request.Alignment!.Value);
    // ... as before
})
.AddEndpointFilter<ValidationFilter<CreateHeroRequest>>()
.RequireAuthorization();
```

`Api/HeroContracts.cs`:

```csharp
public sealed record HeroResponse(
    int Id,
    string Owner,
    string Codename,
    // ... as before
{
    public static HeroResponse From(Hero hero) => new(
        hero.Id, hero.OwnerName, hero.Codename, /* ... as before */);
}
```

</details>

## Task 3 — every write needs a user

`[Authorize]` on `Register`, `AddPower`, `Retire` and `Delete` in `HeroesApiController`, on the `register` endpoint in `HeroEndpoints` (`.RequireAuthorization()`), and on both `Create` actions in `PowersController`. Reads (`GET`) stay anonymous.

<details>
<summary>Hint (open only if you're stuck)</summary>

The same pattern on every action that changes data:

```csharp
[Authorize]
[HttpPost("{id:int}/register")]
public async Task<ActionResult<HeroResponse>> Register(int id)
```

`Microsoft.AspNetCore.Authorization` is the namespace for `[Authorize]`. In `HeroEndpoints.cs`, `.RequireAuthorization()` goes at the end of the `register` endpoint, after the closing `})`.

</details>

## Task 4 — My drafts

1. `HeroesController.Drafts()`: `[Authorize]`, `[HttpGet]`; the model is `store.Heroes.OwnedBy(User.Identity!.Name!).Drafts()` projected to `HeroListItemViewModel`.
2. `Views/Heroes/Drafts.cshtml`: a heading, the `TempData["Message"]` paragraph (copy it from `Index.cshtml`), "No drafts yet." when the list is empty, and a `<ul>` of codenames.
3. `Create` (POST) redirects to `Drafts` instead of `Index`.
4. `Views/Shared/_Layout.cshtml`: a link "My drafts" inside the `@if (User.Identity?.IsAuthenticated == true)` block, before the user name.

<details>
<summary>Hint (open only if you're stuck)</summary>

`HeroesController`:

```csharp
[Authorize]
[HttpGet]
public IActionResult Drafts()
{
    var drafts = store.Heroes.OwnedBy(User.Identity!.Name!).Drafts()
        .Select(h => new HeroListItemViewModel(h.Id, h.Codename))
        .ToList();
    return View(drafts);
}
```

`Views/Heroes/Drafts.cshtml`, the list part:

```html
@model IReadOnlyList<HeroListItemViewModel>

@if (Model.Count == 0)
{
    <p>No drafts yet.</p>
}

<ul>
    @foreach (var hero in Model)
    {
        <li>@hero.Codename</li>
    }
</ul>
```

`_Layout.cshtml`, inside the `@if` block:

```html
<a asp-controller="Heroes" asp-action="Drafts">My drafts</a>
```

</details>

## Check

`HeroRegistry.http`, section "Day 4: tokens", from top to bottom: the title of each request says the expected status. Then in the browser: log in as alice, create a Draft, and it is on `/Heroes/Drafts`; log out, log in as bob: "No drafts yet."

## Done when...

- `POST api/heroes` without a token → **401** with `WWW-Authenticate: Bearer`; with alice's token → **201** and `"owner": "alice"`.
- `POST api/minimal/heroes` behaves the same way.
- Every write in the API returns 401 without a token; `GET api/heroes` still works without one.
- `/Heroes/Create` and `/Heroes/Drafts` redirect an anonymous browser to `/Account/Login?ReturnUrl=...`.
- A Draft saved from the form has the signed-in user as its owner and appears only in that user's `/Heroes/Drafts`.
- The Heroes table has an `OwnerName` column (SQL Server Object Explorer).

## Help, if you get stuck

- **`Update-Database` before the lab says `There is already an object named 'Heroes'`**: your database was created by different migrations than the ones in this project. Run `Drop-Database`, then `Update-Database`. You lose only your own data from Day 3; the seed heroes are created again.
- **`There is no argument given that corresponds to the required parameter 'alignment'`**: the new first parameter shifted the others; add the owner as the first argument in every `new Hero(...)`.
- **`Add-Migration` says the model has pending changes**: you changed the mapping after the migration; `Remove-Migration`, then add it again.
- **API still returns 302 to `/Account/Login` instead of 401**: the request path doesn't start with `/api`, or `ForwardDefaultSelector` in `Program.cs` was changed.
- **401 with a token**: `WWW-Authenticate: Bearer error="invalid_token"` means the token was created for another machine or with another key. Create it again and paste the whole token. Check the header: `Authorization: Bearer <token>` with one space.
- **`User.Identity.Name` is null in the Minimal API**: the endpoint has no `RequireAuthorization()`, so an anonymous request got in.
- **Drafts page shows other users' heroes**: the query is missing `OwnedBy(...)`.

## Bonus — optional, only if time allows

1. **Controller-level `[Authorize]`.** Put `[Authorize]` on `HeroesApiController` itself and `[AllowAnonymous]` on the two `GET` actions. Same behaviour, less repetition.
2. **Who am I?** `GET /Account/Me` (`[Authorize]`) that renders every claim of the current user (`User.Claims`, type and value). Compare the claims from the cookie with the claims from a JWT: call the API version `GET api/me` with a token. Name and role have the same long claim types in both (`.../claims/name`, `.../claims/role`): `JwtBearer` renames the token's `unique_name` and `role`. The token adds `aud`, `iss`, `exp` and a few more.
3. **An expired token.** `dotnet user-jwts create --name bob --valid-for 10s -o token`, wait ten seconds, and use it. Which status and which `WWW-Authenticate` value does the API return?

**Important:** the reference solution will be published in this folder as `Solution/` after the lab. Lab 02 starts from the reference version, no matter how far you got.
