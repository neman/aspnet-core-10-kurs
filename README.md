# ASP.NET Core 10 — course

Lab materials for the ASP.NET Core 10 course (Belgrade, October 2026).

## Prerequisites

- **.NET 9 SDK** — check with `dotnet --version` in the repository folder (expected `9.0.x`). `global.json` in the repository root selects the .NET 9 SDK even if a newer one is installed; if it reports that no compatible SDK was found, install the .NET 9 SDK. It installs side by side with .NET 10.
- an editor or IDE of your choice: Visual Studio 2022 version 17.12 or later, VS Code with C# Dev Kit, or Rider
- `curl` or a browser to call the endpoints (in Windows PowerShell use `curl.exe`)
- from Day 3: **SQL Server Express LocalDB**, installed with the Visual Studio workload *ASP.NET and web development*. Check with `sqllocaldb start MSSQLLocalDB` (expected `LocalDB instance "MSSQLLocalDB" started.`)

The course covers ASP.NET Core 10, but all lab code also runs on .NET 9, so the projects target .NET 9 (`net9.0`) and open in Visual Studio 2022. If you use Visual Studio 2026 or .NET 10, keep your setup: install the .NET 9 SDK next to .NET 10 and leave the projects as they are.

## Structure

| Folder | When | Content |
|---|---|---|
| `Modul-1/Presentation/` | Day 1 | Module 1 slides as a single HTML file; open it in a browser, works offline |
| `Modul-1/Lab-01-DI/` | Day 1, 09:45–10:15 | Dependency injection: Email/SMS selection through a factory, and DI lifetimes |
| `Modul-1/Lab-02-Middleware/` | Day 1, 11:12–11:47 | Custom middleware, an extension method, and execution order |
| `Modul-2/Presentation/` | Day 2 | Module 2 slides as a single HTML file; open it in a browser, works offline |
| `Modul-2/Lab-01-MVC-Heroes/` | Day 2, 09:35–10:19 | Controller, ViewModel, strongly typed views and layout: hero list and details |
| `Modul-2/Lab-02-Form-Validation/` | Day 2, 10:54–11:38 | New hero form, validation, antiforgery and Post/Redirect/Get |
| `Modul-2/Optional/` | Day 2, 12:00–13:00 | Optional self-paced tasks: editing a Draft hero, View Component, client-side validation, styling and static files, an action filter |
| `Modul-3/Presentation/` | Day 3 | Module 3 slides as a single HTML file; open it in a browser, works offline |
| `Modul-3/Lab-01-EF-Core/` | Day 3, 09:25–10:05 | EF Core on SQL Server (LocalDB): `DbContext` as the store, mapping, migrations, `Include` |
| `Modul-3/Lab-02-Web-API/` | Day 3, 10:38–11:18 | JSON API: the hero lifecycle, domain rules as ProblemDetails; the same API as Minimal API |
| `Modul-3/Lab-03-SignalR/` | Day 3, 11:28–11:43 | SignalR: a hero list that updates without a refresh |
| `Modul-3/Optional/` | Day 3, 12:00–13:00 | Optional self-paced tasks: Options, lifecycle buttons in MVC, SQLite, the whole lifecycle as Minimal API, health check |

Each lab folder has its instructions in `README.md` and the project in a subfolder. Every lab starts from its own starter, so none depends on whether the previous one was finished. Reference solutions are published in the `Solution/` folder after the lab.

## Running a project

```bash
cd Modul-1/Lab-01-DI/CourseApi
dotnet build
dotnet run
```

You can also run any project from Visual Studio with **F5**. Both F5 and `dotnet run` use the profile from `Properties/launchSettings.json`, which is the same in all labs: `http://localhost:5100` and `https://localhost:5101`, Development environment. Since the port is the same, only one application can run at a time. Requests in the `.http` files use `http`, so you don't need the development certificate; for the `https` address, Visual Studio offers to trust it on first run (or run `dotnet dev-certs https --trust`). If the application fails to start with `Unable to configure HTTPS endpoint`, the machine has no development certificate: run `dotnet dev-certs https --trust` and start the application again.

The endpoints for each lab are listed in its instructions. Stop the application with `Ctrl+C` (in Visual Studio, **Stop**).
