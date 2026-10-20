using HeroRegistry.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// In-memory data: the database is a Singleton and lives until the app restarts;
// the store is Scoped, one unit of work per request, like DbContext.
builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddScoped<IHeroStore, InMemoryHeroStore>();

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
