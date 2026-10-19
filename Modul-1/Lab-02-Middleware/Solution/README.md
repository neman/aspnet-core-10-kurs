# Lab 02 — reference solution

Complete solution of tasks A–C from `../README.md`. It contains everything from the starter, plus:

- `Middleware/RequestLoggingMiddleware.cs`: logs the request path **before** `next` and the response status **after** it, with structured `ILogger<T>` messages.
- `Extensions/RequestLoggingMiddlewareExtensions.cs`: the `UseRequestLogging()` extension method.
- A second, inner inline middleware in `Program.cs` that logs `Inner before` / `Inner after`.

Run: `dotnet run` or **F5** in Visual Studio. Call `GET http://localhost:5100/orders/confirm`. Expected HTTP 200 and this log order:

```text
Before /orders/confirm
Inner before
Order confirmation requested
SMS: Order confirmed
Inner after
After 200
```

`GET /health` and `GET /di/lifetime` still work. The launch profile runs in Development, so the channel is Sms. The `SMS: ...` line is written by `Console.WriteLine` and may appear in a different place than the log messages.

The optional tasks (short-circuit, configuration value, `Stopwatch`, Serilog) are not included; their reference solutions are in `../README.md`. Use this project as their starting point.
