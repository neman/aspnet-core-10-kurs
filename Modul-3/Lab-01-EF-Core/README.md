# Lab 01 — EF Core: the in-memory store becomes SQL Server

**Time:** 09:25–10:05 (40 min, including review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` project in this folder. It is the result of the instructor's demo: the EF Core packages, `Data/HeroDbContext.cs` that maps only `Power`, the migration `InitialCreate`, and a connection string to the database `HeroRegistry_Lab1` on LocalDB. `GlobalUsings.cs` makes the namespaces `HeroRegistry.Data`, `HeroRegistry.Models` and `Microsoft.EntityFrameworkCore` available in every file, so new code needs no `using` lines for them. The application still uses yesterday's in-memory store.

## Before you start

- **LocalDB.** In a terminal: `sqllocaldb start MSSQLLocalDB` prints `LocalDB instance "MSSQLLocalDB" started.` You can also connect from SQL Server Management Studio: server `(localdb)\MSSQLLocalDB`, Windows Authentication. It is installed with the Visual Studio workload *ASP.NET and web development*. If it is missing, work in a pair and install *SQL Server Express LocalDB* later from the Visual Studio Installer (Individual components).
- **No LocalDB, but SQL Server is installed:** in `appsettings.json`, change only `Server=` and add `TrustServerCertificate=True`. The server name is `localhost` for a default instance or `localhost\SQLEXPRESS` for SQL Server Express (in JSON the backslash is doubled). In SQL Server Object Explorer and Management Studio, connect to the same server.

  ```json
  "HeroRegistry": "Server=localhost\\SQLEXPRESS;Database=HeroRegistry_Lab1;Trusted_Connection=True;TrustServerCertificate=True"
  ```

  Run `git restore appsettings.json` before the next `git pull`.
- **Package Manager Console:** Tools → NuGet Package Manager → Package Manager Console, *Default project*: `HeroRegistry`. Commands like `Add-Migration` and `Update-Database` are the same as in EF6.
- **Without Visual Studio:** `dotnet tool install --global dotnet-ef --version 9.0.21`, then `dotnet ef migrations add ...` and `dotnet ef database update` from the project folder.
- **Don't update the EF Core packages to 10.x.** EF Core 10 needs .NET 10, and the project targets `net9.0`.

## Goal

Replace the in-memory store with EF Core. `HeroDbContext` maps `Hero` as well, implements `IHeroStore`, and replaces `InMemoryHeroStore` in DI. The controllers and views **do not change**: they already talk to `IHeroStore`, which was shaped like `DbContext` on purpose.

## Plan

- **First 3 min**: run `Update-Database` and look at the database (task 0).
- **Next 8 min**: task 1 (mapping `Hero`).
- **Next 8 min**: task 2 (`HeroDbContext` as `IHeroStore`).
- **Next 5 min**: task 3 (seed data).
- **Next 5 min**: task 4 (migration).
- **Next 6 min**: task 5 (run and check).
- **Last 5 min**: review with the instructor.

## Task 0 — the database from the demo

1. Package Manager Console: `Update-Database`.
2. View → SQL Server Object Explorer → `(localdb)\MSSQLLocalDB` → Databases → `HeroRegistry_Lab1` → Tables. There are `Powers` (Shield, Light, Flight, from `HeroSeed`) and `__EFMigrationsHistory`.
3. Run the application (F5): `/Heroes` shows Aurora and Sentinel from memory, like yesterday, and `/Powers` shows `Flight (memory)`, `Light (memory)`, `Shield (memory)`. The database has the same powers without the label: the application doesn't read it yet.

## Task 1 — map `Hero`

In `HeroDbContext.OnModelCreating`, next to `Power`, add the configuration for `Hero`:

- table `Heroes`
- `Codename` at most 60 characters, `OriginStory` at most 1000
- `Race`, `Alignment` and `Status` stored as text: `.HasConversion<string>().HasMaxLength(20)`
- powers: `hero.HasMany(h => h.Powers).WithMany().UsingEntity(join => join.ToTable("HeroPowers"));`

You don't have to do anything for the private constructor, the `private set` properties or the `_powers` field: EF Core uses them by convention. Think about why the table name has to be written: without a `DbSet<Hero>` property, what would EF Core call the table?

<details>
<summary>Hint (open only if you're stuck)</summary>

The same shape as the `Power` configuration above it:

```csharp
modelBuilder.Entity<Hero>(hero =>
{
    hero.ToTable("Heroes");
    hero.Property(h => h.Codename).HasMaxLength(60);
    hero.Property(h => h.OriginStory).HasMaxLength(1000);

    hero.Property(h => h.Race).HasConversion<string>().HasMaxLength(20);
    hero.Property(h => h.Alignment).HasConversion<string>().HasMaxLength(20);
    hero.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);

    hero.HasMany(h => h.Powers)
        .WithMany()
        .UsingEntity(join => join.ToTable("HeroPowers"));
});
```

</details>

## Task 2 — `HeroDbContext` becomes the `IHeroStore`

1. `HeroDbContext` also implements `IHeroStore`: `: DbContext(options), IHeroStore`.
2. Add the missing members:
   - `public IQueryable<Hero> Heroes => Set<Hero>();`. Leave it exactly like this for now; you come back to it in task 5.
   - `Add(Hero)`, `Add(Power)` and `Remove(Hero)` call `Set<Hero>().Add(...)`, `Set<Power>().Add(...)` and `Set<Hero>().Remove(...)`.
   - `SaveChanges()` already exists: `DbContext` has it.
3. In `Program.cs`, delete the two in-memory registrations and add, after `AddDbContext`:

```csharp
builder.Services.AddScoped<IHeroStore>(sp => sp.GetRequiredService<HeroDbContext>());
```

4. Delete `Services/InMemoryDatabase.cs` and `Services/InMemoryHeroStore.cs`. Build: no controller should need a change.

<details>
<summary>Hint (open only if you're stuck)</summary>

`Powers` and `SaveChanges()` are already there; the interface needs these members as well:

```csharp
public sealed class HeroDbContext(DbContextOptions<HeroDbContext> options)
    : DbContext(options), IHeroStore
{
    public IQueryable<Hero> Heroes => Set<Hero>();

    public void Add(Hero hero) => Set<Hero>().Add(hero);
    public void Add(Power power) => Set<Power>().Add(power);
    public void Remove(Hero hero) => Set<Hero>().Remove(hero);

    // Powers, OnModelCreating: as before
}
```

If `IHeroStore` is not found, add `using HeroRegistry.Services;` at the top of the file.

</details>

## Task 3 — seed data

Replace the body of `HeroSeed.Seed` so that it seeds the same heroes as yesterday, through the same domain methods:

```csharp
public static void Seed(DbContext db)
{
    if (db.Set<Hero>().Any())
        return;

    var sentinel = new Hero(
        "Sentinel",
        "A human engineer dedicated to protecting the city.",
        HeroRace.Human,
        HeroAlignment.Heroic);
    sentinel.AddPower(GetPower(db, "Shield"));
    sentinel.Register();

    var aurora = new Hero(
        "Aurora",
        "An alien explorer who defends her adopted home.",
        HeroRace.Alien,
        HeroAlignment.Heroic);
    aurora.AddPower(GetPower(db, "Light"));
    aurora.AddPower(GetPower(db, "Flight"));
    aurora.Register();

    var shadowling = new Hero(
        "Shadowling",
        "An apprentice still working on their origin story.",
        HeroRace.Mutant,
        HeroAlignment.Neutral);

    db.Set<Hero>().AddRange(sentinel, aurora, shadowling);
    db.SaveChanges();
}

// Reuses a power that is already in the database; a new one is saved with its hero.
private static Power GetPower(DbContext db, string name) =>
    db.Set<Power>().FirstOrDefault(p => p.Name == name) ?? new Power(name);
```

## Task 4 — migration

1. `Add-Migration AddHeroes`
2. Open `Migrations/..._AddHeroes.cs`. Find `CreateTable` for `Heroes` and `HeroPowers`, the foreign keys, and the `Status` column as `nvarchar(20)`.
3. `Update-Database`. Seeding runs after the migration.
4. SQL Server Object Explorer: `Heroes` has 3 rows, and the `Status` column contains `Registered` and `Draft`, not numbers.

## Task 5 — run and check

1. `/Heroes` → Aurora, Sentinel. The application console now shows the SQL that EF Core runs.
2. `/Heroes/Details/2` → **where are Aurora's powers?** Explain why, then fix `Heroes` in `HeroDbContext` so that it always loads the powers.
3. Create a new hero with the form, stop the application and start it again. The new Draft is in the `Heroes` table.

<details>
<summary>Hint (open only if you're stuck)</summary>

EF Core has no lazy loading by default: a navigation like `Powers` is loaded only when the query asks for it. Look at the SQL for `/Heroes/Details/2` in the console: there is no `HeroPowers`. `Register()` also needs the powers, so the store always loads them:

```csharp
public IQueryable<Hero> Heroes => Set<Hero>().Include(h => h.Powers);
```

</details>

## Done when...

- `/Heroes` shows Aurora and Sentinel from the database, and `/Heroes/Details/3` returns 404.
- `/Heroes/Details/2` shows Light and Flight, and `/Powers` has no "(memory)" label: the data comes from SQL Server, and no controller was changed.
- The database has the tables `Heroes`, `Powers`, `HeroPowers`, and two rows in `__EFMigrationsHistory`.
- A new Draft survives a restart.
- `HeroesController` is unchanged.

## Help, if you get stuck

- **`Add-Migration` is not recognized**: the Package Manager Console has the wrong *Default project*, or Visual Studio has to be restarted after the restore.
- **`Unable to create a 'DbContext' of type ...`**: the project doesn't build. Run Build first and fix the errors.
- **`'HeroDbContext' does not implement interface member 'IHeroStore.Heroes'`**: the property is a `DbSet<Hero>`, but the interface wants exactly `IQueryable<Hero>`.
- **`The model for context 'HeroDbContext' has pending changes`**: you changed the mapping after the last migration. Run `Add-Migration` before `Update-Database`.
- **`Cannot open database "HeroRegistry_Lab1"`**: you haven't run `Update-Database` yet.
- **The table is called `Hero`**: `ToTable("Heroes")` is missing.
- **The heroes are missing although the tables exist**: seeding runs only on `Update-Database`, and only when `Heroes` is empty. Delete the rows, or run `Drop-Database` and then `Update-Database`.
- **Unable to resolve service for type `InMemoryDatabase`**: one of the old registrations is still in `Program.cs`.
- **`/Heroes` returns 500 with only a `traceId`**: the details are in the application's console window.

## Bonus — optional, only if time allows

1. **Which SQL?** In the console, compare the SQL for `/Heroes` and for `/Heroes/Details/2`. Why doesn't the list read `OriginStory` and doesn't join `HeroPowers`, although `Heroes` has `Include`?
2. **Unique codename.** `hero.HasIndex(h => h.Codename).IsUnique();`, then `Add-Migration UniqueCodename` and `Update-Database`. Create two heroes with the same name through the form. What happens, and where should this rule be checked so that the user gets a message instead of a 500?
3. **SQL for the DBA.** `Script-Migration` (or `dotnet ef migrations script`) writes all migrations as one SQL script. When is that better than `Update-Database`?

**Important:** the reference solution will be published in this folder as `Solution/` after the lab. It uses the database `HeroRegistry`, so it doesn't collide with your migrations. Lab 02 starts from the reference version, no matter how far you got.
