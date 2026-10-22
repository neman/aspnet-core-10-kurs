# Lab 02 — The hero lifecycle through a JSON API

**Time:** 10:38–11:18 (40 min, including review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` project in this folder. It is the Lab 01 solution plus the instructor's Demo 2: `HeroesApiController` with list, details, create and register, the DTOs in `Api/HeroContracts.cs`, and `DomainExceptionHandler`, which turns broken domain rules into ProblemDetails (400 or 409). `Api/HeroEndpoints.cs` has the same four endpoints as a Minimal API under `api/minimal/heroes`; the tasks below are for the controller.

```bash
cd Modul-3/Lab-02-Web-API/HeroRegistry
```

Create the database first. Package Manager Console (*Default project*: `HeroRegistry`): `Update-Database`, or `dotnet ef database update`. The database is called `HeroRegistry`; the labs from now on all use it.

Then run the project with **F5** or `dotnet run` (`http://localhost:5100`). All requests are in `HeroRegistry.http`; the Lab 2 section at the bottom is the check for this lab. To look around or try a request in the browser, open `http://localhost:5100/scalar/v1`.

## Goal

A client can take a hero through its whole lifecycle over HTTP: create a Draft, add a power, register, retire, or delete a Draft. Every rule lives in `Hero`. The controller only finds the hero, calls one domain method, saves, and returns a DTO. A broken rule throws a domain exception, and `DomainExceptionHandler` turns it into a response; the controller doesn't catch anything.

## Plan

- **First 5 min**: `Update-Database`, run the app, send the Demo 2 requests from `HeroRegistry.http`, and read `HeroesApiController.Register`.
- **Next 7 min**: task 1 (forbidden phrases).
- **Next 8 min**: task 2 (add a power).
- **Next 7 min**: task 3 (retire).
- **Next 6 min**: task 4 (delete).
- **Next 2 min**: the check.
- **Last 5 min**: review with the instructor.

## Task 1 — forbidden phrases (domain)

From the task: the origin story of a Registered hero must not contain "tragic past", "chosen one" or "mysterious organization", ignoring case.

1. In `Hero`, add a list of the phrases (for example `public static readonly IReadOnlyList<string> ForbiddenPhrases`).
2. In `Register()`, after the length check: if the story contains one of them (`Contains(phrase, StringComparison.OrdinalIgnoreCase)`), throw a `BusinessRuleException` whose message names the phrase.

You can only check this after task 2, because a hero needs a power before it can be registered.

<details>
<summary>Hint (open only if you're stuck)</summary>

```csharp
public static readonly IReadOnlyList<string> ForbiddenPhrases =
    ["tragic past", "chosen one", "mysterious organization"];
```

In `Register()`, after the length check:

```csharp
var phrase = ForbiddenPhrases.FirstOrDefault(
    p => OriginStory.Contains(p, StringComparison.OrdinalIgnoreCase));
if (phrase is not null)
    throw new BusinessRuleException($"The origin story must not contain \"{phrase}\".");
```

</details>

## Task 2 — add a power (API)

`POST api/heroes/{id}/powers/{powerId}` adds an existing power to the hero. Powers in the database: 1 Shield, 2 Light, 3 Flight.

- `[HttpPost("{id:int}/powers/{powerId:int}")]`
- find the hero (`FindAsync`) and the power (`store.Powers.FirstOrDefaultAsync(...)`); if either is missing → `NotFound()`
- `hero.AddPower(power)`, `await store.SaveChangesAsync()`, return `HeroResponse.From(hero)`

<details>
<summary>Hint (open only if you're stuck)</summary>

```csharp
[HttpPost("{id:int}/powers/{powerId:int}")]
public async Task<ActionResult<HeroResponse>> AddPower(int id, int powerId)
{
    var hero = await FindAsync(id);
    var power = await store.Powers.FirstOrDefaultAsync(p => p.Id == powerId);
    if (hero is null || power is null)
        return NotFound();

    hero.AddPower(power);
    await store.SaveChangesAsync();

    return HeroResponse.From(hero);
}
```

</details>

## Task 3 — retire (domain + API)

1. `Hero.Retire()`: only a Registered hero can be retired; otherwise throw `InvalidStatusException`. Then the status is `Retired`. `AddPower` already refuses changes to a Retired hero.
2. `POST api/heroes/{id}/retire`, in the same shape as `Register`.

<details>
<summary>Hint (open only if you're stuck)</summary>

```csharp
public void Retire()
{
    if (Status != HeroStatus.Registered)
        throw new InvalidStatusException(Status, "Only a Registered hero can be retired.");

    Status = HeroStatus.Retired;
}
```

The action is `Register` with `[HttpPost("{id:int}/retire")]` and `hero.Retire()`.

</details>

## Task 4 — delete (domain + API)

1. `Hero.EnsureCanBeDeleted()`: throws `InvalidStatusException` unless the hero is a Draft.
2. `[HttpDelete("{id:int}")]`: find the hero, `hero.EnsureCanBeDeleted()`, `store.Remove(hero)`, save, and return `NoContent()` (204).

<details>
<summary>Hint (open only if you're stuck)</summary>

```csharp
public void EnsureCanBeDeleted()
{
    if (Status != HeroStatus.Draft)
        throw new InvalidStatusException(
            Status,
            "Only a Draft hero can be deleted. Retire a Registered hero instead.");
}
```

```csharp
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
{
    var hero = await FindAsync(id);
    if (hero is null)
        return NotFound();

    hero.EnsureCanBeDeleted();
    store.Remove(hero);
    await store.SaveChangesAsync();

    return NoContent();
}
```

`IActionResult`, not `ActionResult<HeroResponse>`: there is no body to return.

</details>

## Check

Send the requests in the Lab 2 section of `HeroRegistry.http` from top to bottom. The title of each request says which status it should return. The requests use `{{newHero.response.body.$.id}}`, the id from the response of the request named `newHero`. If your client doesn't support that, write the id directly (4 on a fresh database).

## Done when...

- Nightwatch goes Draft → Registered → Retired, and every step returns 200 with the new status.
- Deleting a Registered hero, retiring a Draft, retiring twice and adding a power to a Retired hero return **409**.
- Registering a story with "The Chosen One" returns **400** and names the phrase in `detail`.
- Deleting a Draft returns **204**, and after that `GET` returns 404.
- An unknown hero or power returns 404.
- No action has a `try`/`catch`, and no action returns `BadRequest` or `Conflict` for a domain rule.

## Help, if you get stuck

- **No LocalDB** (`Update-Database` fails with "Unable to locate a Local Database Runtime installation"): in `appsettings.json`, change only `Server=` to your SQL Server and add `TrustServerCertificate=True`. The server name is `localhost` for a default instance or `localhost\SQLEXPRESS` for SQL Server Express (in JSON the backslash is doubled):

  ```json
  "HeroRegistry": "Server=localhost\\SQLEXPRESS;Database=HeroRegistry;Trusted_Connection=True;TrustServerCertificate=True"
  ```

  Run `git restore appsettings.json` before the next `git pull`.
- **405 Method Not Allowed for `DELETE api/heroes/5`**: the route exists only for `GET`; the `[HttpDelete]` action is missing.
- **404 for `POST .../powers/1`**: check the route template; `{id:int}` and `{powerId:int}` must match the action's parameter names.
- **500 instead of 400 or 409**: you threw an exception that is not a `DomainException`, for example `InvalidOperationException`. The details are in the application console.
- **Register says "needs at least one power" although you added one**: `AddPower` didn't save; `await store.SaveChangesAsync()` is missing.
- **"chosen one" passes**: `Contains` without `StringComparison.OrdinalIgnoreCase` is case-sensitive.
- **A Retired hero is still public**: the list uses `Registered()`. Check that `Retire()` really sets `Status`.

## Bonus — optional, only if time allows

1. **Remove a power.** `Hero.RemovePower(Power)`: a Retired hero is read-only (409), and a Registered hero must keep at least one power (400). Then `DELETE api/heroes/{id}/powers/{powerId}`. Check: remove Shield from Sentinel → 400; add Light to Sentinel and remove Shield → 200.
2. **XML and content negotiation.** In `Program.cs`, add `.AddXmlSerializerFormatters()` after `AddControllersWithViews()`. Send `GET api/heroes/2` with the header `Accept: application/xml`. Which format comes back, and why? Then set `options.ReturnHttpNotAcceptable = true` in `AddControllersWithViews(options => ...)` and repeat. What about `GET api/heroes/999`?
3. **Retire as a Minimal API endpoint** (after task 3). In `Api/HeroEndpoints.cs`, add `POST api/minimal/heroes/{id}/retire` in the same shape as the `register` endpoint there. Check: retire a Registered hero through `api/minimal/heroes/...` → 200, and retire it again → 409 with the same `detail` as the controller gives.

**Important:** the reference solution will be published in this folder as `Solution/` after the lab. Lab 03 starts from the reference version, no matter how far you got.
