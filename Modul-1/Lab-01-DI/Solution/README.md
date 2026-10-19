# Lab 01 — reference solution

Complete solution of tasks 1–4 from `../README.md`. The bonus task (`MarkerCache`) is not included.

Run: `dotnet run` or **F5** in Visual Studio. Endpoints: `GET /health`, `GET /orders/confirm`, `GET /di/lifetime`.

- **Tasks 1–3:** `SmsSender` is the second `INotificationSender` implementation. A factory registration in `Program.cs` reads `Notifications:Channel` and returns `SmsSender` for `Sms`, otherwise `EmailSender`. `OrderService` is unchanged. The channel is set in `appsettings.json` (`Email`); `appsettings.Development.json` does not override it. To test SMS, change the value to `Sms` and restart the application.
- **Task 4:** `InstanceMarker` is registered as **Scoped**, `FirstConsumer` and `SecondConsumer` as Transient. For the other variants, replace only the `InstanceMarker` registration with `AddTransient<InstanceMarker>()` or `AddSingleton<InstanceMarker>()` and restart. Keep only one registration at a time.
