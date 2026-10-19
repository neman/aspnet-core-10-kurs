# Lab 02 — HTTP middleware in CourseApi (ASP.NET Core 10)

**Time:** 11:12–11:47 (35 minutes) · **Format:** individually or in pairs · **Starting code:** `CourseApi/` in this folder

## Goal

Build a middleware that writes logs before and after an HTTP request is processed, register it through an extension method, and explain how the pipeline executes. Keep the existing services and endpoints working.

## Starting state

The starter already contains `OrderService`, Email/Sms channel selection through `Notifications:Channel`, `ILogger<OrderService>`, and the DI lifetime example. The endpoints are `GET /health`, `GET /orders/confirm`, `GET /orders/{id:int}/confirm`, `GET /di/lifetime` and `GET /pipeline-demo`.

**Do not create a new project.** You don't need to finish the DI lab to start this one.

## Running

In the starter project folder:

```bash
cd Modul-1/Lab-02-Middleware/CourseApi
dotnet restore
dotnet build
dotnet run
```

Or open the project in Visual Studio and run it with **F5**. Both use the profile from `Properties/launchSettings.json`, so the address is the same: `http://localhost:5100`. The requests for checking are also in `CourseApi/CourseApi.http`.

In a second terminal, call:

```bash
curl -i http://localhost:5100/health
```

In Windows PowerShell, use `curl.exe` if `curl` is an alias for `Invoke-WebRequest`.

## Task A — Middleware and extension method (about 16 min)

1. Create a `RequestLoggingMiddleware` class in a separate file.
2. Receive `RequestDelegate` and `ILogger<RequestLoggingMiddleware>` through the constructor.
3. In `InvokeAsync(HttpContext context)`, log the request path as a structured log message before calling the next component.
4. Call `await _next(context)`.
5. After that, log the HTTP status of the response.
6. Create an extension method `UseRequestLogging(this IApplicationBuilder app)` that uses `UseMiddleware<RequestLoggingMiddleware>()`.
7. Register the middleware **once** in `Program.cs`, before the endpoints execute.

**Success criterion:** `GET /orders/confirm` returns 200, and the console contains roughly `Before /orders/confirm` and `After 200`.

## Task B — Execution order (about 10 min)

1. After `app.UseRequestLogging()`, add a second, **inline** middleware.
2. Name its messages `Inner before` and `Inner after`; call `next` between them.
3. **Before running**, write down the order of the four middleware messages.
4. Call `GET /orders/confirm`, check the console, and compare the result with your prediction.

**Success criterion:** you can explain why `Inner after` appears before the outer `After 200` message. Logs from `OrderService` may appear between them.

**Note:** the `Email: ...` and `SMS: ...` lines are written by `Console.WriteLine`, which writes immediately, while the logger writes through a background queue. That is why they may appear in the console in a different place than you expect. Compare only the middleware log messages.

## Task C — Verification (about 6 min)

```bash
curl -i http://localhost:5100/orders/confirm
curl -i http://localhost:5100/di/lifetime
curl -i http://localhost:5100/health
```

Check: both key endpoints work, the scoped `InstanceMarker` gives `Same: true` within a request, the logs are in the expected order, and a successful request has status 200.

## If you get stuck

- `UseRequestLogging` is not available: check the namespace and the `using` in `Program.cs`.
- No logs: check the position of `UseRequestLogging()` and whether you restarted the application.
- The build fails: check the `InvokeAsync` signature and whether `_next(context)` is called.
- Different GUIDs are not an error in themselves. What matters is the `Same` field and the chosen lifetime.

The reference solution for tasks A–C is in `Solution/` in this folder; the optional tasks below are not included. Nobody depends on finishing this exercise to start Module 2.

## Optional (12:00–13:00)

- **A — Short-circuit:** a middleware responds to `GET /blocked` with 403 itself and does not pass the request on.
- **B — A single configuration value:** `GET /notifications/channel` returns only the active channel.
- **C — Request duration:** `RequestLoggingMiddleware` measures the duration with `Stopwatch` and logs `{ElapsedMs}`.
- **D — Serilog:** logs go to the console and to a text file.

Each task below has steps, a check, and a reference solution. Pick one and finish it. **Starting state** for all of them: your `CourseApi` from this lab or the reference solution from `Solution/`.

**All additional tasks are optional.** The required session ends at 12:00; the instructor does not have to be present until 13:00. Tomorrow everyone uses the separate `HeroRegistry` starter.

## Optional A — Short-circuit: a middleware that doesn't call `next` (about 10 min)

**Goal:** see what happens when a middleware writes the response itself. So far, every middleware has called `await next(context)` and handed the request to the next component. If it doesn't call `next`, the request ends there: the components after it and the endpoint do not run. This is called a *short-circuit*. `UseStaticFiles` (returns a file) and `UseAuthorization` (returns 401/403) work the same way.

In .NET Framework, the equivalent is an `HttpModule` that writes the response in `BeginRequest` and calls `HttpApplication.CompleteRequest()`.

### Steps

1. **Before writing code**, predict: which of the four messages (`Before`, `Inner before`, `Inner after`, `After`) will appear for `GET /blocked`? The `/blocked` endpoint doesn't exist. What status would you get without the new middleware?
2. In `Program.cs`, add an inline middleware (`app.Use`) **after** `app.UseRequestLogging()` and **before** the `Inner` middleware from Task B.
3. If `context.Request.Path` equals `/blocked`: set `context.Response.StatusCode` to `StatusCodes.Status403Forbidden`, write the text `Blocked by middleware` with `context.Response.WriteAsync(...)`, and exit with `return`, **without** calling `next`.
4. For all other paths, call `await next(context)`.
5. Restart the application and call:

   ```bash
   curl -i http://localhost:5100/blocked
   curl -i http://localhost:5100/orders/confirm
   ```

### Check

- `/blocked` returns **403** with the body `Blocked by middleware`, not 404.
- The console shows only `Before /blocked` and `After 403`; `Inner before` and `Inner after` do not appear.
- `/orders/confirm` still returns 200 with all four messages, as before.

### Questions to think about

1. Why does `After 403` still appear, even though nothing after the new middleware was called?
2. What would you see in the console if you had placed the new middleware **before** `app.UseRequestLogging()`?

<details>
<summary>Reference solution (open only when you're done or stuck)</summary>

```csharp
app.UseRequestLogging();

// Short-circuit: /blocked never reaches the inner middleware or an endpoint.
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/blocked")
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Blocked by middleware");
        return;
    }

    await next(context);
});

// Inner middleware from Task B stays here.
```

**Answers:**

1. `RequestLoggingMiddleware` called `await _next(context)`, and that call returned normally, just earlier. After it returns, it logs the status that the new middleware wrote.
2. None of our messages. `RequestLoggingMiddleware` would be after the component that ends the request, so it would never run.

</details>

## Optional B — A single configuration value (about 10 min)

**Goal:** read a configuration value in an endpoint and see that the same value differs between the Development and Production environments, without changing code.

### Steps

1. In `Program.cs`, add the endpoint `GET /notifications/channel`.
2. Let the endpoint receive `IConfiguration` as a parameter; DI injects it just like `OrderService`.
3. Return JSON with **only one** field: `Channel`, the value of the `Notifications:Channel` key.
4. Run the application the usual way (F5 or `dotnet run`, Development environment) and call:

   ```bash
   curl -i http://localhost:5100/notifications/channel
   ```

5. Stop the application and run it without the launch profile, which makes the environment Production:

   ```bash
   dotnet run --no-launch-profile --urls http://localhost:5100
   ```

   Call the same URL again.

### Check

- Development: `{"channel":"Sms"}` (value from `appsettings.Development.json`).
- Production: `{"channel":"Email"}` (value from `appsettings.json`).
- At startup, the console shows `Hosting environment: Development` or `Production`, respectively.

### Questions to think about

1. Why not return the whole configuration, for example all key–value pairs?
2. In PowerShell, set `$env:Notifications__Channel = "Push"` and start the application from the **same** terminal. What does the endpoint return and why?

<details>
<summary>Reference solution (open only when you're done or stuck)</summary>

```csharp
// Return a single, harmless configuration value.
app.MapGet("/notifications/channel", (IConfiguration config) =>
    Results.Ok(new { Channel = config["Notifications:Channel"] }));
```

**Answers:**

1. The configuration also contains connection strings, keys, and passwords from user secrets or environment variables. An endpoint should return only what is safe to show.
2. `Push`. Environment variables have higher priority than `appsettings*.json`, and `__` in the name replaces `:`. Remove the variable afterwards with `Remove-Item Env:Notifications__Channel`.

</details>

## Optional C — Request duration with `Stopwatch` (about 15 min)

**Goal:** extend `RequestLoggingMiddleware` so it logs how long request processing took. Middleware is the right place for this, because it wraps everything that runs after it.

### Steps

1. In `RequestLoggingMiddleware.cs`, add `using System.Diagnostics;`.
2. In `InvokeAsync`, after the `Before` message, start measuring: `var watch = Stopwatch.StartNew();`.
3. Put the `await _next(context)` call and the `After` message in a `try` block.
4. In the `finally` block, stop the measurement and log the structured message `Request took {ElapsedMs} ms` with `watch.ElapsedMilliseconds`.
5. Restart the application and call `/orders/confirm` a few times.

### Check

- After `After 200`, `Request took N ms` appears, where N is zero or a positive number. The first request is usually slower than the rest.
- The HTTP responses are the same as before; the middleware only measures.
- `dotnet build` passes without warnings.

### Challenge — why `finally`

Temporarily add an endpoint that throws an exception:

```csharp
app.MapGet("/boom", () =>
{
    throw new InvalidOperationException("Boom");
});
```

Call `/boom`. Which messages from `RequestLoggingMiddleware` appear? Then move the duration log from `finally` to after the `try` block and repeat. Finally, remove `/boom`.

<details>
<summary>Reference solution (open only when you're done or stuck)</summary>

`Middleware/RequestLoggingMiddleware.cs`:

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CourseApi.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Before {Path}", context.Request.Path);
        var watch = Stopwatch.StartNew();

        try
        {
            await _next(context);
            _logger.LogInformation("After {StatusCode}", context.Response.StatusCode);
        }
        finally
        {
            watch.Stop();
            _logger.LogInformation("Request took {ElapsedMs} ms", watch.ElapsedMilliseconds);
        }
    }
}
```

**Answer to the challenge:** for `/boom`, `Before /boom` and `Request took N ms` appear, but not `After`. The exception propagates out of `await _next(context)`, so the rest of the `try` block is skipped, while `finally` always runs. The exception is then caught by the developer exception page, which ASP.NET Core places at the start of the pipeline in the Development environment, and it returns 500. Without `finally` there would be no duration message either, and slow and failed requests are exactly the ones whose duration we want to see.

</details>

## Optional D — Serilog: console and text file (about 25 min, can also be homework)

**Goal:** logs go to the console and to a file through Serilog, while `OrderService` and `RequestLoggingMiddleware` don't change a single line. The code writes to `ILogger<T>`, and the provider decides where the entry ends up.

**Starting state:** your `CourseApi` from this lab or the reference solution from `Solution/`. The NuGet package requires internet access.

### Steps

1. In the project folder, add the package (in Visual Studio: right-click the project → **Manage NuGet Packages** → `Serilog.AspNetCore`):

   ```bash
   dotnet add package Serilog.AspNetCore
   ```

   Verified with `Serilog.AspNetCore` 10.0.0, which also works on .NET 9. The package already includes the Console and File sinks and reading settings from configuration, so no other packages are needed.

2. In `Program.cs`, add `using Serilog;`. Right after `WebApplication.CreateBuilder(args)`, register Serilog through `builder.Services.AddSerilog(...)` so that it reads its settings from configuration with `ReadFrom.Configuration(builder.Configuration)`.
3. In `appsettings.json`, add a `"Serilog"` section:
   - `MinimumLevel`: `Default` is `Information` and `Microsoft.AspNetCore` is `Warning`, same as in the existing `Logging` section
   - `WriteTo`: an array with two sinks, `Console` and `File`; the File sink gets `Args` with `"path": "logs/courseapi-.log"` and `"rollingInterval": "Day"`
4. Run the application (F5 or `dotnet run`) and call:

   ```bash
   curl -i http://localhost:5100/orders/42/confirm
   ```

### Check

- The console uses the Serilog format, for example `[10:15:02 INF] Before /orders/42/confirm`.
- The project folder contains `logs/courseapi-YYYYMMDD.log`; `rollingInterval` adds the date to the name. The file contains `Before /orders/42/confirm`, `Order 42 confirmed` and `After 200`, with the full date and time.
- `OrderService` and the middleware are unchanged, and `dotnet build` passes without warnings.

### Challenge — structured log in JSON

Add a **second** File sink with the path `logs/courseapi-.json` and the formatter `"Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"` (the `formatter` key in `Args`). Call `/orders/42/confirm` again.

**Check:** the line for that order contains `"@mt":"Order {OrderId} confirmed"`, `"OrderId":42` and `"RequestPath":"/orders/42/confirm"`. `OrderId` is a separate, searchable field, not just part of the text. That is why the message template uses `{OrderId}` instead of interpolation.

### Questions to think about

1. In `appsettings.Development.json`, change `Logging:LogLevel:Default` to `Warning` and restart the application. Did the Information logs disappear? Why?
2. The JSON line contains `RequestPath`, even though you didn't log it. Where does it come from?
3. The `SMS: Order 42 confirmed` line now sits exactly between `Order 42 confirmed` and `Inner after`, while with the built-in logger it could appear elsewhere. What changed?

### If you get stuck

- **`AddSerilog` doesn't exist:** `using Serilog;` is missing or the package wasn't added (check the `PackageReference` in the `.csproj`).
- **No file and no error:** Serilog silently skips a sink with a wrong name (for example `"Files"`). Check that `WriteTo` is an array and that it says `"Name": "File"`.
- **The file is in an unexpected place:** the relative path `logs/...` is resolved from the working directory. F5 and `dotnet run` from the project folder create it in the project folder.
- **Logs in Git:** `logs/` is already in this repository's `.gitignore`.

<details>
<summary>Reference solution (open only when you're done or stuck)</summary>

`Program.cs`, beginning of the file; the rest is unchanged:

```csharp
using CourseApi.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog replaces the default providers; sinks and levels come from the "Serilog" section.
builder.Services.AddSerilog(logger => logger.ReadFrom.Configuration(builder.Configuration));
```

`appsettings.json`, including the challenge (second File sink):

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": { "Microsoft.AspNetCore": "Warning" }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": { "path": "logs/courseapi-.log", "rollingInterval": "Day" }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/courseapi-.json",
          "rollingInterval": "Day",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ]
  },
  "Notifications": { "Channel": "Email" },
  "AllowedHosts": "*"
}
```

**Answers:**

1. No. After `AddSerilog`, the levels are set by `Serilog:MinimumLevel`. The `Logging` section applies to the built-in providers, which are no longer used.
2. ASP.NET Core opens a logging scope with `RequestId` and `RequestPath` for every request, and Serilog writes the scope properties as fields of the entry.
3. The built-in console logger writes through a background queue, while the Serilog Console sink writes immediately, just like `Console.WriteLine`.

</details>
