# Optional 12:00–13:00 — self-paced tasks

This hour is not required and Day 3 doesn't depend on it. Pick one or more tasks; each has a check, and the reference solution is in `Solution/`. Look at it only when you're done or stuck.

**Starting point:** the `HeroRegistry/` project in this folder (the Lab 02 solution). The domain already has everything for task A: the query `store.Heroes.Drafts()` and the method `hero.UpdateDraft(...)`, which works only for a Draft hero.

```bash
cd Modul-2/Optional/HeroRegistry
dotnet run
```

Or open the project in Visual Studio and run it with **F5**. Both use the profile from `Properties/launchSettings.json`, so the address is the same: `http://localhost:5100`.

## Task A — editing a Draft hero (30–40 min)

A new hero is a Draft and is not publicly visible. Build pages where Drafts can be seen and edited.

1. A `Drafts()` action and a `Views/Heroes/Drafts.cshtml` view: a list of Draft heroes, each with an "edit" link to `/Heroes/Edit/{id}`. Add a "Drafts" link to the layout.
2. `[HttpGet] Edit(int id)`: `store.Heroes.Drafts().FirstOrDefault(h => h.Id == id)`; for `null`, return `NotFound()`. Fill a `CreateHeroInputModel` with the hero's values and return the view.
3. `[HttpPost] [ValidateAntiForgeryToken] Edit(int id, CreateHeroInputModel input)`: check again that the hero is a Draft, then the same validation as in `Create`; on success call `hero.UpdateDraft(...)` and `store.SaveChanges()`, put a message in `TempData`, and redirect to `Drafts`.
4. The form fields are the same as in `Create.cshtml`. Move them to a partial view `Views/Heroes/_HeroFields.cshtml` and use `<partial name="_HeroFields" />` in both forms.

**Check:**

- `/Heroes/Drafts` shows Shadowling.
- `/Heroes/Edit/3` shows a filled-in form; `/Heroes/Edit/1` (Registered) and `/Heroes/Edit/999` return 404.
- An empty `Codename` returns the form with an error; a valid edit returns 302 to `/Heroes/Drafts` and the new value is in the list.
- The edited Draft is still not visible in `/Heroes`.

**Question to think about:** the edit form has `asp-action="Edit"` without `asp-route-id`, yet it posts to `/Heroes/Edit/3`. How does the Tag Helper know the `id`?

## Task B — View Component (15 min)

Show the number of Registered heroes in the layout, on every page.

1. `ViewComponents/HeroCountViewComponent : ViewComponent`, with `IHeroStore` through the constructor and an `Invoke()` method that returns `View(count)`.
2. A `Views/Shared/Components/HeroCount/Default.cshtml` view with `@model int`.
3. Add `@addTagHelper *, HeroRegistry` to `_ViewImports.cshtml`, then call `<vc:hero-count></vc:hero-count>` in the layout.

**Check:** every page shows "2 registered heroes". A new Draft doesn't change the number.

**Question to think about:** why isn't this a partial view? What does a View Component have that a partial doesn't?

## Task C — client-side validation (15 min, internet required)

1. Create `Views/Shared/_ValidationScriptsPartial.cshtml` with three scripts, in this order:
   - `https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js`
   - `https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.21.0/jquery.validate.min.js`
   - `https://cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/4.0.0/jquery.validate.unobtrusive.min.js`
2. In the layout, before `</body>`, add `@await RenderSectionAsync("Scripts", required: false)`.
3. In `Create.cshtml` (and `Edit.cshtml`, if you did A), add `@section Scripts { <partial name="_ValidationScriptsPartial" /> }`.

**Check:**

- An empty form shows the errors without sending a request; there is no POST in the Network tab.
- View Page Source: the fields have `data-val-required` attributes. Who wrote them?
- Turn off JavaScript (DevTools → Ctrl+Shift+P → "Disable JavaScript") and submit an empty form: the server still rejects the input. Client-side validation is a convenience, not protection.

## Task D — styling and static files (10–15 min)

Add the ready-made style from `assets/site.css` to the application. You don't write any CSS: it targets plain elements and the classes that Tag Helpers already write into the HTML (`input-validation-error`, `field-validation-error`, `validation-summary-errors`), so you don't change any view. This task doesn't depend on A–C.

1. In the project, create the folder `wwwroot/css` and copy `assets/site.css` from this folder into it.
2. In `Views/Shared/_Layout.cshtml`, in `<head>`, after `<title>`, add:

   ```html
   <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
   ```

3. Run the application and open `/Heroes`. There is no styling, and `/css/site.css` returns 404, even though the file exists.
4. Stop the application and add static file serving to `Program.cs`:

   ```csharp
   var app = builder.Build();

   app.MapStaticAssets();
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}")
       .WithStaticAssets();
   ```

5. Run the application again.

**Check:**

- The pages are styled: purple navigation, the hero list in a frame, validation errors in red, the message after saving in green.
- `/css/site.css` returns 200; `/appsettings.json` and `/HeroRegistry.csproj` return 404.
- View Page Source: the link is `href="/css/site.css?v=..."`. The `~/` path and the `v` parameter were written by the Tag Helper.
- In `site.css`, change `--accent: #6c4ce0;` to another color and restart the application: both the color and the `v` value change.

**Questions to think about:**

- In step 3 the link already has `?v=...`, so the Tag Helper read the file from disk. Why did the request still return 404?
- In classic ASP.NET, IIS serves every file in the site folder except those it explicitly blocks (`web.config`, `bin`, `App_Data`). Here only `wwwroot` is served. Which is safer?
- What is `?v=` for? What would happen if the browser cached the CSS and you then changed it?

## Task E — action filter (15–20 min)

Write a filter that logs, for every MVC action, its name, its duration, and whether `ModelState` is valid. This task doesn't depend on A–D.

1. Create `Filters/ActionTimingFilter.cs`:

   ```csharp
   using System.Diagnostics;
   using Microsoft.AspNetCore.Mvc.Filters;

   namespace HeroRegistry.Filters;

   public sealed class ActionTimingFilter(ILogger<ActionTimingFilter> logger) : IAsyncActionFilter
   {
       public async Task OnActionExecutionAsync(
           ActionExecutingContext context, ActionExecutionDelegate next)
       {
           var start = Stopwatch.GetTimestamp();

           await next();

           // TODO: logger.LogInformation with context.ActionDescriptor.DisplayName,
           // Stopwatch.GetElapsedTime(start).TotalMilliseconds and context.ModelState.IsValid
       }
   }
   ```

2. Register it globally in `Program.cs`:

   ```csharp
   builder.Services.AddControllersWithViews(options =>
       options.Filters.Add<ActionTimingFilter>());
   ```

3. Run the application and open `/`, `/Heroes`, then submit an empty and a valid new hero form. Watch the console the application runs in (in Visual Studio: the application's console window or Output → ASP.NET Core Web Server).
4. Make the filter run only for `HeroesController`. Remove the global registration from `Program.cs` (go back to `AddControllersWithViews()`) and put `[ServiceFilter(typeof(ActionTimingFilter))]` on the `HeroesController` class. Run the application and open `/Heroes`. Then add `builder.Services.AddScoped<ActionTimingFilter>();` and try again.

**Check:**

- After step 3 the console shows, for example: `HeroRegistry.Controllers.HeroesController.Create (HeroRegistry) took 0.3 ms, ModelState valid: False` for the empty form and `... valid: True` for the valid one.
- For `/css/site.css` (if you did D), `/doesnotexist`, and a POST without the antiforgery token (the last request in `HeroRegistry.http`) there is **no** line in the log.
- After step 4 without `AddScoped`: `/Heroes` returns 500 with the message "No service for type 'HeroRegistry.Filters.ActionTimingFilter' has been registered.". With `AddScoped`, only `HeroesController` is logged, and `/` no longer is.

**Questions to think about:**

- Why is there no log line for a POST without the token? Who stopped the request, and before your filter?
- The filter sees `ModelState` before the action runs. What has already been done by then?
- The duration is under a millisecond, even though the page has HTML. When does the view execute?
- Why did `[ServiceFilter]` require a DI registration, while `options.Filters.Add<ActionTimingFilter>()` didn't?
- The same thing could be measured with the middleware from Day 1. What does a filter know that middleware doesn't?

## If you get stuck

- **`<vc:hero-count>` appears literally in the HTML** — `@addTagHelper *, HeroRegistry` is missing (the assembly name, not the namespace).
- **`InvalidOperationException: The view 'Components/HeroCount/Default' was not found`** (in the console) — check the path and the folder name `HeroCount`, without the `ViewComponent` suffix.
- **The `Scripts` section is not rendered** — the layout has no `RenderSectionAsync("Scripts", ...)`.
- **Client-side validation doesn't work** — check in the Console tab that all three scripts loaded and that the order is correct.
- **`/css/site.css` still returns 404** — the file is not in `wwwroot/css` (check the folder name), there is no `app.MapStaticAssets()`, or the application wasn't restarted after the change.
- **The style doesn't change after editing the CSS** — restart the application and refresh the page with Ctrl+F5.
- **The filter logs nothing** — check that it is registered (globally or with `[ServiceFilter]` on the controller) and that it calls `logger.LogInformation`, not `LogDebug`; `appsettings.json` shows level `Information` and above.
- **Every action is logged twice** — the filter is registered both globally and through `[ServiceFilter]`.
