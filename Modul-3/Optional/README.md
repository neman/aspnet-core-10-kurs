# Optional 12:00–13:00 — self-paced tasks

This hour is not required and Day 4 doesn't depend on it. Pick one or more tasks; each has a check, and the reference solution is in `Solution/`. Look at it only when you're done or stuck.

**Starting point:** the `HeroRegistry/` project in this folder (the Lab 03 solution). It uses the database `HeroRegistry` on LocalDB; if it doesn't exist on your machine yet, run `Update-Database` first. All requests below are also at the bottom of `Solution/HeroRegistry.http`.

## Task A — forbidden phrases in configuration (15 min)

The phrases are a list in code, so a new one needs a new release. Move them to `appsettings.json` with the Options pattern.

1. `Services/RegistrationRulesOptions.cs`: a class with `public string[] ForbiddenPhrases { get; set; } = [];` and a constant `SectionName = "RegistrationRules"`.
2. `appsettings.json`: a section `"RegistrationRules": { "ForbiddenPhrases": [ ... ] }` with the three phrases from the task and a fourth one, `"secret lab accident"`.
3. `Program.cs`: `builder.Services.Configure<RegistrationRulesOptions>(builder.Configuration.GetSection(RegistrationRulesOptions.SectionName));`
4. `Hero`: a new overload `Register(IEnumerable<string> forbiddenPhrases)` holds the checks; `Register()` calls it with the default list, so the seed data keeps working.
5. The `register` action receives `[FromServices] IOptionsSnapshot<RegistrationRulesOptions> rules` and calls `hero.Register(rules.Value.ForbiddenPhrases)`.

**Check:** a Draft "Labrat" with the story "Born in a secret lab accident beneath the river city.", plus a power: register → 400 and `detail` names "secret lab accident". Change the phrase in `appsettings.json` while the app is running and register again: what happens without a restart?

**Question to think about:** why does the domain receive a list of phrases instead of `IOptions<...>`?

## Task B — the lifecycle from MVC pages (30–40 min)

The same domain methods, but from HTML forms instead of the API.

1. `HeroesController.Drafts()` and `Views/Heroes/Drafts.cshtml`: a list from `store.Heroes.Drafts()`. Next to each Draft, two forms with one button each: **Register** and **Delete** (`method="post"`, `asp-action`, `asp-route-id`). Add a "Drafts" link to the layout.
2. POST actions `Register(int id)`, `Delete(int id)` and `Retire(int id)`, each with `[ValidateAntiForgeryToken]`. Find the hero; for `null` return `NotFound()`. Call the domain method, save, put a message in `TempData`, and redirect.
3. Catch `DomainException` in these actions and put `ex.Message` in `TempData`, so the user sees the rule on the page.
4. A **Retire** button on `Details.cshtml`.

**Check:**

- `/Heroes/Drafts` shows Shadowling. Register → back on Drafts with "A hero needs at least one power."
- Delete Shadowling → the message, and it's gone from the list.
- Retire Sentinel from its details page → `/Heroes` shows only Aurora.

**Questions to think about:** without the `try`/`catch`, what would the browser get from the same exception, and who would write it? A hero registered from the Drafts page doesn't appear live on `/Heroes` in another window. Why, and where should the notification be sent so that it works for both the API and MVC?

## Task C — SQLite as a second provider (30 min)

The same model on another database. Migrations hold SQL for one provider, so SQLite gets its own context type and its own migrations.

1. Package `Microsoft.EntityFrameworkCore.Sqlite` **9.0.21**.
2. `HeroDbContext`: no longer `sealed`, and the primary constructor becomes two constructors: `public HeroDbContext(DbContextOptions<HeroDbContext> options) : base(options)` and `protected HeroDbContext(DbContextOptions options) : base(options)`.
3. `Data/SqliteHeroDbContext.cs`: `public sealed class SqliteHeroDbContext(DbContextOptions<SqliteHeroDbContext> options) : HeroDbContext(options);`
4. `appsettings.json`: `"Database": { "Provider": "SqlServer" }` and a connection string `"HeroRegistrySqlite": "Data Source=HeroRegistry.db"`.
5. `Program.cs`: if `builder.Configuration["Database:Provider"] == "Sqlite"`, register `AddDbContext<HeroDbContext, SqliteHeroDbContext>(options => options.UseSqlite(...).UseSeeding(...))`, otherwise the SQL Server registration you already have.
6. Migrations for SQLite (Package Manager Console):

```powershell
Add-Migration InitialCreate -Context SqliteHeroDbContext -OutputDir Migrations\Sqlite -Args "--Database:Provider=Sqlite"
Update-Database -Context SqliteHeroDbContext -Args "--Database:Provider=Sqlite"
```

   Or: `dotnet ef migrations add InitialCreate --context SqliteHeroDbContext --output-dir Migrations/Sqlite -- --Database:Provider=Sqlite`.

7. Set `"Provider": "Sqlite"` and run the app.

**Check:** `/Heroes` and `GET api/heroes` show Aurora and Sentinel from `HeroRegistry.db` in the project folder. Compare `Migrations/Sqlite/..._InitialCreate.cs` with the SQL Server migrations: what is different?

**Note:** from now on there are two context types, so SQL Server commands need the context too: `Update-Database -Context HeroDbContext`. Don't commit `HeroRegistry.db`.

## Task D — the whole lifecycle as a Minimal API (25–30 min)

`Api/HeroEndpoints.cs` already has list, details, create and register under `api/minimal/heroes`. Add the rest of Lab 02, so the Minimal API can do everything the controller can.

1. `POST /{id:int}/powers/{powerId:int}`: 404 if the hero or the power is missing, otherwise `AddPower`, save, `TypedResults.Ok(...)`.
2. `POST /{id:int}/retire` and `DELETE /{id:int}` (`MapDelete`, 204 with `TypedResults.NoContent()`), in the same shape as `register`. Declare every possible result in the return type, for example `Task<Results<NoContent, NotFound>>`.
3. The controller sends SignalR messages after `register` and `retire`. Do the same: add `IHubContext<HeroHub> hub` as a parameter and call `hub.Clients.All.SendAsync(...)` after the save.
4. If you did task A: `register` receives `IOptionsSnapshot<RegistrationRulesOptions> rules`, like the controller action.

**Check:** a new Draft through `POST api/minimal/heroes`, add Shield, register → 200 and the hero appears on an open `/Heroes` page; delete it → 409; retire → 200 and it disappears from `/Heroes`. Delete a Draft → 204. In `/openapi/v1.json`, compare `/api/minimal/heroes/{id}` with `/api/heroes/{id}`: which one lists the 404?

**Questions to think about:** the 404 from the Minimal API has an empty body, and the one from the controller is ProblemDetails. Why? The parameter `IHeroStore store` has no attribute: how does the Minimal API know it's a service? Why didn't `DomainExceptionHandler` need a single change?

## Task E — health check (10 min)

On Day 1, `/health` was a lambda that always returned OK. Now it checks the database.

1. Package `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` **9.0.21**.
2. `builder.Services.AddHealthChecks().AddDbContextCheck<HeroDbContext>();` and `app.MapHealthChecks("/health");`

**Check:** `GET /health` → 200 `Healthy`. Stop the app, change `Database=HeroRegistry` to `Database=Missing` in the connection string, and start it again: `/` still returns 200, but `/health` returns 503 `Unhealthy`. Change the name back.

**Question to think about:** why doesn't stopping LocalDB (`sqllocaldb stop MSSQLLocalDB`) make the check fail?

## If you get stuck

- **`More than one DbContext was found`** (after task C): add `-Context HeroDbContext` or `-Context SqliteHeroDbContext`.
- **`SQLite Error 1: 'no such table: Heroes'`**: the SQLite migration wasn't applied; run `Update-Database -Context SqliteHeroDbContext -Args "--Database:Provider=Sqlite"`.
- **Version conflict when adding a package**: choose 9.0.21, not the latest 10.x.
- **The POST from the Drafts page returns 400**: the form has no antiforgery token. It needs `method="post"` and `asp-action`; then the Form Tag Helper adds the token.
- **Task A: the new phrase is ignored**: the action still calls `hero.Register()` without arguments, or the section name in `appsettings.json` doesn't match `SectionName`.
