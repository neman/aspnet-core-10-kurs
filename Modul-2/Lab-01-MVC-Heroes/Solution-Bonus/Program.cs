using HeroRegistry.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// In-memory data: the database is a Singleton and lives until the app restarts;
// the store is Scoped, one unit of work per request, like DbContext.
builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddScoped<IHeroStore, InMemoryHeroStore>();

var app = builder.Build();

// Bonus 4: an empty 400-599 response is re-executed as /Home/Status/{code}; URL and status stay.
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
