# Lab 01 — Dependency Injection in CourseApi

**Time:** 09:45–10:15 (30 min, including review)  
**Format:** individually or in pairs.  
**Starting point:** the `CourseApi/` project in this folder (works without any previous exercise).

```bash
cd Modul-1/Lab-01-DI/CourseApi
dotnet run
```

Or open the project in Visual Studio and run it with **F5**. Both use the profile from `Properties/launchSettings.json`, so the address is the same: `http://localhost:5100`.

In Windows PowerShell, use `curl.exe` or a browser for the calls: `http://localhost:5100/health`. In Visual Studio, all requests for this lab are in `CourseApi/CourseApi.http` (**Send request** above each request).

## Goal
Switch the notification implementation through configuration without changing `OrderService`, then run an experiment to see how the DI lifetime affects instance sharing.

## Plan
- **First 2 min** — read the tasks and run the project; check `/health` and `/orders/confirm`.
- **Next 12 min** — tasks 1–3 (Email/SMS and the factory).
- **Next 12 min** — task 4 (lifetime experiment).
- **Last 4 min** — review the result with the instructor.

## Task 1 — a second implementation
Create `SmsSender : INotificationSender` that writes `SMS: {message}` to the console. Do not change the existing `OrderService`.

## Task 2 — selection from configuration
In `appsettings.json`, use `Notifications:Channel` with the values `Email` and `Sms`. Instead of registering `EmailSender` directly in DI, implement a **factory** for `INotificationSender` that reads the configuration and picks the matching implementation. Keep `OrderService` dependent on the interface only.

## Task 3 — functional check
Call `GET /orders/confirm` for both channels. Expected: HTTP 200 with `"Order confirmed"`, and `Email: Order confirmed` or `SMS: Order confirmed` in the console. After changing the configuration, **restart the application** so the check is predictable. Note: the starter `appsettings.Development.json` does not set this key.

## Task 4 — how long does an instance live?
Create `InstanceMarker` with a `Guid Id` property, and two consumers, `FirstConsumer` and `SecondConsumer`, that receive `InstanceMarker` **through the constructor** and expose `MarkerId`. Register the consumers as `Transient`. Add `GET /di/lifetime`, which receives both consumers as handler parameters and returns `{ first, second, same }`.

Register the marker as **Scoped** first, then try **Transient**, then **Singleton** (one registration at a time; restart after each change). Call the endpoint twice for each variant.

| Registration | Within one request | Between two requests, without restarting |
|---|---|---|
| Transient | `First != Second` | New instances |
| Scoped | `First == Second` | New Id |
| Singleton | `First == Second` | Same Id |

## Done when...
- Email/SMS selection works without changing `OrderService`.
- `GET /di/lifetime` shows the expected result for all three lifetimes.
- The endpoint does not use `IServiceProvider` to resolve instances manually.

## Help, if you get stuck
- How do I read the configuration? `IConfiguration` is already registered in DI, so the factory can request it from the `IServiceProvider` it receives as a parameter. A nested key is read with a colon: `config["Notifications:Channel"]`. Configuration is covered in detail after the break.
- `Unable to resolve service`? Check the DI registrations.
- Always Email? Check the `Notifications:Channel` key, its spelling, and the active environment.
- Scoped gives two IDs? Check that both consumers are in the same HTTP request.
- Confusing results? Change only one registration and restart the application between lifetime variants.

## Bonus — optional, only if time allows
Add a `MarkerCache` class that receives `InstanceMarker` through the constructor and exposes `MarkerId`. Register it as **Singleton** and leave the marker as **Scoped**. Add `GET /di/captive`, which receives `MarkerCache` and `InstanceMarker` as handler parameters and returns `{ cached, current }`.

1. Run the application as before (`dotnet run` or F5). What happens and why?
2. Run it in the Production environment: `dotnet run --no-launch-profile --urls http://localhost:5100`. Call `/di/captive` twice and compare `cached` and `current`.
3. Explain why this is not a valid dependency graph and why it is not safe for a singleton to hold a scoped service. You don't have to implement the fix.

Done when you can explain both outcomes. After the bonus, restore `Program.cs` or remove `AddSingleton<MarkerCache>()`, because otherwise the application does not start in the Development environment.

**Important:** the reference solution is in `Solution/` in this folder. The bonus is not required for the rest of the course, and the middleware lab starts from its own starter.
