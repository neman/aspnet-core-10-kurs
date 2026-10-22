using HeroRegistry.Api;
using HeroRegistry.Hubs;
using HeroRegistry.Services;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Enums travel as text in JSON ("race": "Alien"); the domain enums carry no attributes.
// Controllers and Minimal APIs (with the OpenAPI document) have separate JSON options.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(
    o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Optional C: the provider comes from configuration; each provider has its own migrations.
if (builder.Configuration["Database:Provider"] == "Sqlite")
{
    builder.Services.AddDbContext<HeroDbContext, SqliteHeroDbContext>(options => options
        .UseSqlite(builder.Configuration.GetConnectionString("HeroRegistrySqlite"))
        .UseSeeding((context, _) => HeroSeed.Seed(context)));
}
else
{
    builder.Services.AddDbContext<HeroDbContext>(options => options
        .UseSqlServer(builder.Configuration.GetConnectionString("HeroRegistry"))
        .UseSeeding((context, _) => HeroSeed.Seed(context)));
}

// The same instance as HeroDbContext, not a second DbContext.
builder.Services.AddScoped<IHeroStore>(sp => sp.GetRequiredService<HeroDbContext>());

// Optional A: forbidden phrases from appsettings.json.
builder.Services.Configure<RegistrationRulesOptions>(
    builder.Configuration.GetSection(RegistrationRulesOptions.SectionName));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddOpenApi();

builder.Services.AddSignalR();

// Optional E: a health check that really opens a database connection.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<HeroDbContext>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi(); // GET /openapi/v1.json
app.MapScalarApiReference(); // UI: /scalar/v1
app.MapHub<HeroHub>("/hubs/heroes");
app.MapHeroEndpoints(); // Optional D: the whole lifecycle
app.MapHealthChecks("/health"); // Optional E

// Serves files from wwwroot: css/site.css from Day 2, optional task D.
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
