# Lab 03 — Prove the rules with tests

**Time:** 11:16–11:50 (30 min of work, then 4 min review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` folder in this lab: a solution `HeroRegistry.sln` with the web project `HeroRegistry/` (the Lab 02 solution) and the test project `HeroRegistry.Tests/` from the instructor's Demo 3, with two passing tests.

```bash
cd Modul-4/Lab-03-Tests/HeroRegistry
dotnet test
```

Expected: `Passed! - Failed: 0, Passed: 2`. In Visual Studio: open the `.sln`, Test → Run All Tests. The integration tests create their own database `HeroRegistry_Tests` on LocalDB; the application does not need to be running.

## Goal

Two kinds of tests. **Unit tests** (`HeroTests`) call the `Hero` class directly: no database, no HTTP. **Integration tests** (`HeroesApiTests`) send real HTTP requests through the whole application, booted in memory by `HeroRegistryFactory` (`WebApplicationFactory<Program>`). A request with the header `X-Test-User: bob` is bob; without the header it is anonymous.

**Naming:** a test name is a sentence a domain expert would understand, words separated by underscores, no method name: `Draft_without_powers_cannot_be_registered`, not `Register_NoPowers_Throws`. Renaming `Register()` must not rename the test.

## Plan

- **First 4 min**: run the tests; read `HeroTests.cs`, `HeroRegistryFactory.cs` and `HeroesApiTests.cs`.
- **Next 8 min**: task 1 (three unit tests).
- **Next 15 min**: task 2 (three integration tests).
- **Last 3 min**: `dotnet test`, then review with the instructor.

## Task 1 — unit tests of the domain

In `HeroTests.cs`, three new facts. Name them yourself, as sentences; the behaviour is:

1. An origin story containing "The Chosen One" (any case) blocks registration, even with a power. The exception is `BusinessRuleException`, its message contains `chosen one`, the status stays `Draft`.
2. A Draft cannot retire: `InvalidStatusException`, and `error.Status` is `HeroStatus.Draft`.
3. A Retired hero is read-only: after `Register()` and `Retire()`, `AddPower` throws `InvalidStatusException`.

A private helper such as `NewDraft(string originStory)` keeps the tests short. `Hero`'s first constructor argument is the owner (`"alice"`).

<details>
<summary>Hint (open only if you're stuck)</summary>

The helper and the first test; the other two have the same shape (arrange, one action, assert):

```csharp
[Fact]
public void Origin_story_with_a_forbidden_phrase_blocks_registration()
{
    var hero = NewDraft("The Chosen One of an ancient order of guardians.");
    hero.AddPower(new Power("Light"));

    var error = Assert.Throws<BusinessRuleException>(() => hero.Register());

    Assert.Contains("chosen one", error.Message);
    Assert.Equal(HeroStatus.Draft, hero.Status);
}

private static Hero NewDraft(string originStory) => new(
    "alice",
    "Nightwatch",
    originStory,
    HeroRace.Human,
    HeroAlignment.Heroic);
```

For a Draft that cannot retire, `Assert.Throws<InvalidStatusException>` returns the exception, so you can check `error.Status`.

</details>

## Task 2 — integration tests

In `HeroesApiTests.cs`. `factory.CreateClientAs(null)` is anonymous, `CreateClientAs("bob")` is bob, `CreateClientAs("alice", "Admin")` is alice with the Admin role. Helpers you may want: `NewHero(codename)` returning a `CreateHeroRequest`, and `CreateDraftAsync(client, codename)` that POSTs it and returns the `HeroResponse`.

1. **Anonymous caller cannot create a hero**: `PostAsJsonAsync("/api/heroes", ...)` → `HttpStatusCode.Unauthorized`, and `response.Headers.WwwAuthenticate` contains the scheme `Bearer`.
2. **Bob cannot delete Alice's draft**: alice creates a Draft, bob sends `DeleteAsync` → `Forbidden`; afterwards alice can still `GET` it (200).
3. **Owner can delete own draft**: alice creates and deletes → `NoContent`; `GET` → `NotFound`.

Each test creates the heroes it needs; the seed heroes (Sentinel 1, Aurora 2 Registered, Shadowling 3 Draft, all alice's) are shared by the whole class, so don't delete them.

<details>
<summary>Hint (open only if you're stuck)</summary>

The two helpers, at the bottom of `HeroesApiTests` (`using HeroRegistry.Models;` for the enums):

```csharp
private static CreateHeroRequest NewHero(string codename) => new()
{
    Codename = codename,
    OriginStory = "A night guard who protects the old harbor district.",
    Race = HeroRace.Human,
    Alignment = HeroAlignment.Heroic
};

private static async Task<HeroResponse> CreateDraftAsync(HttpClient client, string codename)
{
    var response = await client.PostAsJsonAsync("/api/heroes", NewHero(codename));
    response.EnsureSuccessStatusCode();
    return (await response.Content.ReadFromJsonAsync<HeroResponse>())!;
}
```

The 403 test:

```csharp
[Fact]
public async Task Bob_cannot_delete_Alices_draft()
{
    var alice = factory.CreateClientAs("alice");
    var bob = factory.CreateClientAs("bob");
    var draft = await CreateDraftAsync(alice, "Nightwatch");

    var response = await bob.DeleteAsync($"/api/heroes/{draft.Id}");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await alice.GetAsync($"/api/heroes/{draft.Id}")).StatusCode);
}
```

The 401 test has no identity (`CreateClientAs(null)`); `response.Headers.WwwAuthenticate.Single().Scheme` is `"Bearer"`.

</details>

## Check

`dotnet test` → all green, 8 tests. Run it twice: the second run must pass too (the factory recreates the database).

## Done when...

- `HeroTests` has four facts, `HeroesApiTests` four, and every name reads as a sentence about behaviour.
- The 401 test does not send a token; the 403 test sends bob's identity; the 204 test sends alice's.
- No test depends on another test having run first.

## Help, if you get stuck

- **`Cannot open database "HeroRegistry_Tests"`**: LocalDB is not running: `sqllocaldb start MSSQLLocalDB`.
- **All HTTP tests fail with 500**: read the message in the test output; a migration may be missing from the web project (`dotnet build` first).
- **`Assert.Equal() Failure: Expected Forbidden, Actual NotFound`**: you deleted a seed hero in another test, or used an id that doesn't exist. Create the hero inside the test.
- **Expected 401, got 302**: the request went to an MVC page, not to `/api/...`. Only `/api` paths are forwarded to the Bearer scheme.
- **Tests pass alone, fail together**: shared state. Each test should create what it needs and not depend on order.

## Bonus — optional, only if time allows

1. `Registered_hero_cannot_be_deleted_even_by_its_owner`: alice deletes Aurora (id 2) → `Conflict`, and the ProblemDetails `detail` says "Only a Draft hero can be deleted".
2. `Only_an_admin_can_retire_a_hero` (bob → 403) and `Admin_can_retire_a_registered_hero` (alice with role Admin: create, add power 1, register, retire → `Retired`).
3. A test class for the MVC side, `HeroesPagesTests`: anonymous `GET /Heroes/Create` → `Redirect` to `/Account/Login`; bob's `/Heroes/Drafts` contains "No drafts yet" and not "Shadowling".
4. A browser test with Playwright: see `Modul-4/Optional`, task A.

**Important:** the reference solution will be published in this folder as `Solution/` after the lab.
