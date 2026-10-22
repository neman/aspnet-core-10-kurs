using HeroRegistry.Api;
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

builder.Services.AddDbContext<HeroDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("HeroRegistry"))
    .UseSeeding((context, _) => HeroSeed.Seed(context)));

// The same instance as HeroDbContext, not a second DbContext.
builder.Services.AddScoped<IHeroStore>(sp => sp.GetRequiredService<HeroDbContext>());

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi(); // GET /openapi/v1.json
app.MapScalarApiReference(); // UI: /scalar/v1
app.MapHeroEndpoints();

// Serves files from wwwroot: css/site.css from Day 2, optional task D.
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
