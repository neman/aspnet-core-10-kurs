# 05-middleware-lab-starter — ASP.NET Core 10 CourseApi

Use **.NET SDK 10**. This snapshot runs independently of all earlier labs.

```bash
dotnet restore
dotnet build
dotnet run
```

`appsettings.json` defaults to Email; `appsettings.Development.json` overrides the channel to Sms.
The launch profile (F5 in Visual Studio or `dotnet run`) uses `http://localhost:5100` and `https://localhost:5101` with environment Development, so the channel is Sms. `dotnet run --no-launch-profile --urls http://localhost:5100` runs in Production, so the channel is Email.

Endpoints before middleware lab: `GET /health`, `GET /orders/confirm`, `GET /orders/42/confirm`, `GET /di/lifetime`, `GET /pipeline-demo`.

Student starter: DI, configuration, logging and existing endpoints work. **The custom class-based middleware has not been implemented.** The instructor's inline pipeline demo has been removed to prevent duplicate logging.
