using HeroRegistry.Filters;
using HeroRegistry.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// In-memory data: the database is a Singleton and lives until the app restarts;
// the store is Scoped, one unit of work per request, like DbContext.
builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddScoped<IHeroStore, InMemoryHeroStore>();

// Optional task E: [ServiceFilter] takes the filter from DI, so it must be registered.
builder.Services.AddScoped<ActionTimingFilter>();

var app = builder.Build();

// Optional task D: serves files from wwwroot, and only from there.
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
