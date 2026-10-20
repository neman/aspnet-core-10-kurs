# Lab 02 — New hero: form, validation and Post/Redirect/Get

**Time:** 10:54–11:38 (44 min, including review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` project in this folder. It is the Lab 01 solution, so it doesn't matter how far you got in the first lab.

```bash
cd Modul-2/Lab-02-Form-Validation/HeroRegistry
dotnet run
```

Or open the project in Visual Studio and run it with **F5**. Both use the profile from `Properties/launchSettings.json`, so the address is the same: `http://localhost:5100`.

To check status codes, use `HeroRegistry.http`, `curl.exe -i`, or DevTools → Network (turn on **Preserve log** to see both the POST and the GET after the redirect).

## Goal

Add a form for a new hero. The input arrives through a separate input model, the server validates it, invalid input returns the same form with errors, and valid input is saved, followed by a redirect (PRG). A new hero is always **Draft** and does not appear in the public list.

A new hero is created through the constructor `new Hero(codename, originStory, race, alignment)`, which always sets `Draft`. It is saved as in EF Core: `store.Add(hero)`, then `store.SaveChanges()`.

## Plan

- **First 3 min** — run the project, check `/Heroes`, and open `HeroesController`. `/Heroes/Create` returns 404 until you add the form.
- **Next 8 min** — task 1 (input model).
- **Next 14 min** — task 2 (GET and the form).
- **Next 9 min** — task 3 (POST and PRG).
- **Next 5 min** — task 4 (check).
- **Last 5 min** — review with the instructor.

## Task 1 — input model

Create `ViewModels/CreateHeroInputModel`; don't use the `Hero` entity:

- `Codename` — required, at most 60 characters.
- `OriginStory` — not required for a Draft, at most 1000 characters.
- `Race` and `Alignment` — required, of type `HeroRace?` and `HeroAlignment?`.

The model has **no** `Id` and no `Status`. Think about why `Race` and `Alignment` are nullable even though they are required.

The rules for moving to Registered (a story of at least 30 characters, at least one power) are **not** checked here; that is a Day 3 topic.

<details>
<summary>Hint (open only if you're stuck)</summary>

`HeroRace` and `HeroAlignment` are in `HeroRegistry.Models`.

```csharp
using System.ComponentModel.DataAnnotations;
using HeroRegistry.Models;

namespace HeroRegistry.ViewModels;

public sealed class CreateHeroInputModel
{
    [Required, StringLength(60)]
    public string Codename { get; set; } = "";

    [StringLength(1000)]
    [Display(Name = "Origin story")]
    public string? OriginStory { get; set; }

    [Required]
    public HeroRace? Race { get; set; }

    [Required]
    public HeroAlignment? Alignment { get; set; }
}
```

</details>

## Task 2 — GET and the form

1. In `HeroesController`, add `[HttpGet] Create()`, which returns the view with an empty input model.
2. Create `Views/Heroes/Create.cshtml` with `<form asp-action="Create" method="post">`.
3. For each field: `<label asp-for>`, an input element (`<input>`, `<textarea>` or `<select>`) with `asp-for`, and `<span asp-validation-for>`.
4. For `Race` and `Alignment`, use `<select asp-for="..." asp-items="Html.GetEnumSelectList<HeroRace>()">` with an empty option `<option value="">— select —</option>` first.
5. Add `<div asp-validation-summary="All"></div>` above the fields.
6. In `Index.cshtml`, add a "New hero" link to the form.

**Check:** `/Heroes/Create` shows the form; View Page Source contains the hidden field `__RequestVerificationToken`.

<details>
<summary>Hint (open only if you're stuck)</summary>

The GET action returns the view with an empty input model:

```csharp
[HttpGet]
public IActionResult Create() => View(new CreateHeroInputModel());
```

`Views/Heroes/Create.cshtml` needs `@using HeroRegistry.Models` for `GetEnumSelectList<HeroRace>()`. One field with a select list; the empty option comes first, so "nothing selected" sends an empty value:

```cshtml
@model CreateHeroInputModel
@using HeroRegistry.Models

<form asp-action="Create" method="post">
    <div asp-validation-summary="All"></div>

    <label asp-for="Race"></label>
    <select asp-for="Race" asp-items="Html.GetEnumSelectList<HeroRace>()">
        <option value="">— select —</option>
    </select>
    <span asp-validation-for="Race"></span>

    @* the same for Codename (input), OriginStory (textarea) and Alignment (select) *@

    <button type="submit">Save draft</button>
</form>
```

</details>

## Task 3 — POST and Post/Redirect/Get

1. Add `[HttpPost]`, `[ValidateAntiForgeryToken]` and `Create(CreateHeroInputModel input)`.
2. If `ModelState` is not valid, return **the same view with the same model**, so the user doesn't lose their input.
3. If it is valid, create the hero with `new Hero(...)`, call `store.Add(...)` and `store.SaveChanges()`, write a short message to `TempData["Message"]`, and return `RedirectToAction(nameof(Index))`.
4. In `Index.cshtml`, show the message from `TempData`, if there is one.

<details>
<summary>Hint (open only if you're stuck)</summary>

`ModelState` is checked first; only valid input reaches `new Hero(...)`. After the check, `Race` and `Alignment` are not `null`, so `.Value` is safe.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(CreateHeroInputModel input)
{
    if (!ModelState.IsValid)
        return View(input);

    store.Add(new Hero(input.Codename, input.OriginStory ?? "",
        input.Race!.Value, input.Alignment!.Value));
    store.SaveChanges();

    TempData["Message"] = $"Draft '{input.Codename}' saved. Only Registered heroes are public.";
    return RedirectToAction(nameof(Index));
}
```

The message in `Index.cshtml`, above the list:

```cshtml
@if (TempData["Message"] is string message)
{
    <p role="status">@message</p>
}
```

</details>

## Task 4 — check

- Submit an empty form: the same page, errors next to `Codename`, `Race` and `Alignment`, and the text you entered stays in the fields.
- Submit valid input: in the Network tab, the POST returns **302**, then the browser does a GET `/Heroes`. The message is shown once and disappears after F5. The new hero is **not** visible in the list.
- Send a POST without the token (the last request in `HeroRegistry.http`): **400**.

## Done when...

- Invalid input saves nothing and shows the errors next to the fields.
- Valid input returns 302 to `/Heroes` and a message that is shown only once.
- The public list still shows only Aurora and Sentinel.
- A POST without the antiforgery token returns 400.
- The controller receives `CreateHeroInputModel`, not `Hero`.

## Help, if you get stuck

- **The form is empty after an error** — in the POST action you return `View()` instead of `View(input)`.
- **Invalid input is saved anyway** — check that `if (!ModelState.IsValid)` comes before `store.Add(...)`.
- **The hero is not saved** — `store.SaveChanges()` is missing after `store.Add(...)`. As in EF Core, `Add` only prepares the change.
- **`GetEnumSelectList<HeroRace>` doesn't recognize the type** — add `@using HeroRegistry.Models` at the top of the view.
- **`Race` never reports an error** — there is no empty `<option>`, so the browser always sends the first race.
- **400 on a valid form** — the form has no token. Check that `<form>` has `method="post"` and `asp-action`, not a hand-written `action`; then the Form Tag Helper adds the token itself.
- **The message doesn't appear** — you write it to `ViewData` instead of `TempData`; `ViewData` doesn't survive a redirect.

## Bonus — optional, only if time allows

1. Send `Race=999` (edit the request in `HeroRegistry.http` and add a valid token, or change the option value in DevTools). What does the error say, and who wrote it: the attribute or the model binder?
2. Also send `Status=1` with valid input. Did the hero appear in the public list? Why?
3. **Unique codename.** If the `Codename` already exists among the heroes in the store (case-insensitive, and with surrounding spaces), the form returns with the error "This codename is already taken." next to the `Codename` field. Use `store.Heroes.Any(...)` and `ModelState.AddModelError(nameof(input.Codename), "...")`. Check: `aurora` and `Shadowling` give an error, `Nightwatch` passes. Question: why can't this rule be an attribute on `CreateHeroInputModel`?
4. **A custom validation attribute (15 min).** A codename has at most three words. Write an attribute used as `[MaxWords(3)]`.
   - Create `Validation/MaxWordsAttribute.cs`: the class inherits from `ValidationAttribute`, receives the number of words through the constructor, and passes the message `"The {0} field can have at most {1} words."` to the base constructor.
   - Override `IsValid(object? value)`: a value that is not a `string` (including `null`) is valid, because `[Required]` decides whether the field is required. For text, count the words with `Split(' ', StringSplitOptions.RemoveEmptyEntries)`.
   - Override `FormatErrorMessage(string name)` so that `{1}` gets the number of words: `string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, MaxWords)`.
   - Add `MaxWords(3)` to `Codename` next to `[Required, StringLength(60)]`.

   Check: "The Iron Night Watch" returns the form with the error "The Codename field can have at most 3 words."; "Night Watch" passes; an empty form still reports only "The Codename field is required." Questions: why **can** this rule be an attribute, while the unique codename from bonus 3 can't? View Page Source: the `Codename` field has `data-val-required` and `data-val-length`, but nothing for `MaxWords`. What does that mean for client-side validation?
5. **A custom Tag Helper (15–20 min).** On the hero details page, the alignment is shown in color: `Heroic` green, `Villainous` red, everything else gray.
   - Create `TagHelpers/AlignmentBadgeTagHelper.cs`: the class inherits from `TagHelper` (namespace `Microsoft.AspNetCore.Razor.TagHelpers`) and has the property `public string Value { get; set; } = "";`.
   - In `Process(context, output)`: `output.TagName = "span";`, `output.TagMode = TagMode.StartTagAndEndTag;`, a `style` attribute with the color (`output.Attributes.SetAttribute(...)`), and the text `output.Content.SetContent(Value);`.
   - In `_ViewImports.cshtml`, add `@addTagHelper *, HeroRegistry`.
   - In `Views/Heroes/Details.cshtml`, write `<alignment-badge value="@Model.Alignment" />` instead of `@Model.Alignment`.

   Check: `/Heroes/Details/1` shows a green "Heroic", and View Page Source contains `<span ... style="color: ...">Heroic</span>`, without `<alignment-badge>`. Questions: where does the name `alignment-badge` come from, when you never wrote it? What would you use in MVC 5 instead of a Tag Helper?

**Important:** the reference solution will be published in this folder as `Solution/` after the lab. Day 3 starts from the reference version, no matter how far you got.
