using HeroRegistry.Data;
using HeroRegistry.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// TODO Task 2: Delete the two in-memory registrations below and, after AddDbContext,
// register IHeroStore as HeroDbContext (code in README).
// In-memory data: the database is a Singleton and lives until the app restarts;
// the store is Scoped, one unit of work per request, like DbContext.
builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddScoped<IHeroStore, InMemoryHeroStore>();

// EF Core: the DbContext is Scoped, one instance per request.
builder.Services.AddDbContext<HeroDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("HeroRegistry"))
    .UseSeeding((context, _) => HeroSeed.Seed(context)));

var app = builder.Build();

// Serves files from wwwroot: css/site.css from Day 2, optional task D.
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
