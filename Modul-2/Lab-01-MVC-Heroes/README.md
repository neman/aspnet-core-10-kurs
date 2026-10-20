# Lab 01 — Hero list and details in MVC

**Time:** 09:35–10:19 (44 min, including review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` project in this folder (works without any previous exercise).

```bash
cd Modul-2/Lab-01-MVC-Heroes/HeroRegistry
dotnet run
```

Or open the project in Visual Studio and run it with **F5**. Both use the profile from `Properties/launchSettings.json`, so the address is the same: `http://localhost:5100`.

Open `http://localhost:5100/`. To check status codes, use `HeroRegistry.http` (Visual Studio, Rider; VS Code with the REST Client extension), `curl.exe -i` in PowerShell, or DevTools → Network in the browser.

## Goal

Build the public part of the hero registry with MVC: the controller gets data from a service through DI, maps it to a ViewModel, and passes it to a strongly typed view. All pages share a common layout.

The domain is ready and you don't change it:

- `Models/Hero.cs` — `Hero` changes its state only through its own methods (`new Hero(...)` is always Draft, `AddPower`, `Register()`), so its properties have `private set`. A broken rule throws a `DomainException` (`Models/DomainExceptions.cs`).
- `Services/IHeroStore` — similar to an EF Core `DbContext`: you read through `store.Heroes`, while `Add` and `SaveChanges()` are for writing.
- `store.Heroes.Registered()` (from `Models/HeroQueries.cs`) returns only Registered heroes, sorted by `Codename`. The controller needs `using HeroRegistry.Models;` for this method.

Seed data: Sentinel (1, Registered), Aurora (2, Registered), Shadowling (3, Draft). Restarting the application resets the data.

## Plan

- **First 3 min** — run the project, open `/`, and look at `Hero` and `IHeroStore`.
- **Next 12 min** — task 1 (list).
- **Next 12 min** — task 2 (details).
- **Next 9 min** — task 3 (layout).
- **Next 3 min** — task 4 (check).
- **Last 5 min** — review with the instructor.

## Task 1 — hero list

1. Create `Controllers/HeroesController.cs`. It receives `IHeroStore` through the constructor; don't create the store with `new`.
2. Create `ViewModels/HeroListItemViewModel` with `Id` and `Codename` (a record is enough).
3. The `Index()` action maps the heroes from `store.Heroes.Registered()` to a list of ViewModels and returns `View(...)` with that list.
4. Create `Views/Heroes/Index.cshtml` with `@model` and `@foreach`. If the list is empty, show a message instead of an empty `<ul>`.

**Check:** `/Heroes` shows Aurora and Sentinel, in that order.

<details>
<summary>Hint (open only if you're stuck)</summary>

The action maps each `Hero` to a ViewModel. `Registered()` needs `using HeroRegistry.Models;`.

```csharp
public sealed class HeroesController(IHeroStore store) : Controller
{
    public IActionResult Index()
    {
        var heroes = store.Heroes.Registered()
            .Select(h => new HeroListItemViewModel(h.Id, h.Codename))
            .ToList();

        return View(heroes);
    }
}
```

Until task 3 adds `_ViewImports.cshtml`, `@model` needs the full namespace:

```cshtml
@model IReadOnlyList<HeroRegistry.ViewModels.HeroListItemViewModel>

<h1>Registered heroes</h1>

@* if the list is empty, a message; otherwise: *@
<ul>
    @foreach (var hero in Model)
    {
        <li>@hero.Codename</li>
    }
</ul>
```

</details>

## Task 2 — hero details

1. Create `ViewModels/HeroDetailsViewModel` with `Id`, `Codename`, `OriginStory`, `Race`, `Alignment`, and a list of power names.
2. The `Details(int id)` action looks up the hero with `store.Heroes.Registered().FirstOrDefault(h => h.Id == id)`. For a missing or Draft hero it gets `null` and returns `NotFound()`; otherwise it passes the ViewModel to the view.
3. Create `Views/Heroes/Details.cshtml`. Show the powers only if the hero has any.
4. In the list from task 1, each hero becomes a link to its details. For now, write the address by hand (`href="/Heroes/Details/@hero.Id"`); in task 3 you replace it with a Tag Helper.

**Check:** `/Heroes/Details/1` shows Sentinel with the Shield power; `/Heroes/Details/3` (Draft) and `/Heroes/Details/999` return **404**.

<details>
<summary>Hint (open only if you're stuck)</summary>

`Race` and `Alignment` are enums, and `Powers` is a list of `Power` objects, so the ViewModel gets strings:

```csharp
public sealed record HeroDetailsViewModel(
    int Id, string Codename, string OriginStory,
    string Race, string Alignment, IReadOnlyList<string> Powers);
```

```csharp
public IActionResult Details(int id)
{
    var hero = store.Heroes.Registered().FirstOrDefault(h => h.Id == id);
    if (hero is null)
        return NotFound();

    return View(new HeroDetailsViewModel(
        hero.Id, hero.Codename, hero.OriginStory,
        hero.Race.ToString(), hero.Alignment.ToString(),
        hero.Powers.Select(p => p.Name).ToList()));
}
```

The link in the list, and the powers in `Details.cshtml`:

```cshtml
<li><a href="/Heroes/Details/@hero.Id">@hero.Codename</a></li>
```

```cshtml
@if (Model.Powers.Count > 0)
{
    <ul>
        @foreach (var power in Model.Powers)
        {
            <li>@power</li>
        }
    </ul>
}
```

</details>

## Task 3 — layout

1. Create `Views/_ViewImports.cshtml`: `@using HeroRegistry.ViewModels` and `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`. After that, `@model` in views doesn't need the full namespace.
2. Create `Views/Shared/_Layout.cshtml` with `<!doctype html>`, a `<title>` from `ViewData["Title"]`, navigation (Home, Heroes), and `@RenderBody()`.
3. Create `Views/_ViewStart.cshtml` that sets `Layout = "_Layout"`.
4. Remove the page's own `<html>`, `<head>` and `<body>` from `Views/Home/Index.cshtml`; only the page content remains.
5. Write links with Tag Helpers: `asp-controller`, `asp-action`, `asp-route-id` instead of a hand-written `href`.

**Check:** every page has the same navigation; View Page Source starts with `<!doctype html>`; links still lead to the correct addresses.

<details>
<summary>Hint (open only if you're stuck)</summary>

`Views/Shared/_Layout.cshtml`: the frame of every page. The content of the view goes where `@RenderBody()` is.

```cshtml
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>@ViewData["Title"] — Superhero Registry</title>
</head>
<body>
    <nav>
        <a asp-controller="Home" asp-action="Index">Home</a> |
        <a asp-controller="Heroes" asp-action="Index">Heroes</a>
    </nav>
    <main>
        @RenderBody()
    </main>
</body>
</html>
```

`Views/_ViewStart.cshtml` (directly in `Views/`, not in `Shared/`):

```cshtml
@{
    Layout = "_Layout";
}
```

`Views/Home/Index.cshtml` keeps only its content:

```cshtml
@{
    ViewData["Title"] = "Home";
}

<h1>Superhero Registry</h1>
```

The link in the hero list, with Tag Helpers:

```cshtml
<li><a asp-action="Details" asp-route-id="@hero.Id">@hero.Codename</a></li>
```

</details>

## Task 4 — check

Run all requests from `HeroRegistry.http` (or `curl.exe -i http://localhost:5100/Heroes/Details/3`) and compare the status codes with the expected ones.

## Done when...

- `/Heroes` shows only Aurora and Sentinel, with links to their details.
- `/Heroes/Details/1` returns 200, and `/Heroes/Details/3` and `/Heroes/Details/999` return 404.
- All pages use the same layout and start with `<!doctype html>`.
- The controller doesn't create the store with `new` and passes a ViewModel to the view, not the `Hero` entity.

## Help, if you get stuck

- **Status 500** — with F5 or `dotnet run` the application runs in the Development environment, so the error details are on the page in the browser and in the console. (With `--no-launch-profile` it runs in Production: the page is empty and the details are only in the console.) Most often it says `The view 'Index' was not found` and lists the locations MVC tried: check the path `Views/Heroes/Index.cshtml` and the action name.
- **`/Heroes` returns 404** — the class must be named `HeroesController`, be `public`, and inherit from `Controller`.
- **`The type or namespace name 'HeroListItemViewModel' could not be found`** in a view — add the `@using` to `_ViewImports.cshtml` or write the full namespace in `@model`.
- **The layout is not applied** — `_ViewStart.cshtml` must be directly in `Views/`, not in `Views/Shared/`.
- **`asp-action` appears literally in the HTML** — `@addTagHelper` is missing from `_ViewImports.cshtml`.
- **A 404 looks like an empty page in the browser** — that's expected; `NotFound()` returns only the status code. Check it in DevTools → Network.

## Bonus — optional, only if time allows

The bonuses are independent; do any of them.

1. **Attribute route.** Add the attribute route `[HttpGet("heroes/{id:int:min(1)}")]` to `Details`. What do `/heroes/1`, `/heroes/0` and `/Heroes/Details/1` return now? Where do the links from the list lead now? Why?
2. **Partial view.** Move one list row (`<li>` with the link) to `Views/Heroes/_HeroListItem.cshtml` with `@model HeroListItemViewModel`, and call it in the loop with `<partial name="_HeroListItem" model="hero" />`. The list must look the same as before.
3. **Search.** `Index(string? search)` shows only the heroes whose codename contains `search` (case-insensitive). Add a `<form method="get">` with `<input name="search" />` and a button above the list. Check: `/Heroes?search=aur` → only Aurora; `/Heroes?search=shadow` → empty list (a Draft must not be visible here either). Question: where does `search` come from, and why is this form GET and not POST?
4. **A 404 page (10–15 min).** `NotFound()` returns an empty response. Show the user a page in the layout instead.
   - In `Program.cs`, right after `var app = builder.Build();`, add `app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");`.
   - In `HomeController`, add the action `public IActionResult Status(int id) => View(id);`.
   - Create `Views/Home/Status.cshtml` with `@model int`. For `404`, show "Page not found" and a link to the hero list; for other codes, show "Something went wrong" and the code number.

   Check: `/Heroes/Details/3`, `/Heroes/Details/999` and `/doesnotexist` show the page in the layout, while `curl.exe -i` still returns **404** for them. Questions: why does `/doesnotexist` get the same page, even though no controller handles it? Replace `ReExecute` with `UseStatusCodePagesWithRedirects("/Home/Status/{0}")`: which status code does the browser get now, and why is that worse?
**Important:** the reference solution will be published in this folder as `Solution/` after the lab. Lab 02 starts from its own starter, so it doesn't depend on whether you finished this lab.
