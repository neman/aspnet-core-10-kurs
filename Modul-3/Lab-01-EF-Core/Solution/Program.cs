using HeroRegistry.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<HeroDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("HeroRegistry"))
    .UseSeeding((context, _) => HeroSeed.Seed(context)));

// The same instance as HeroDbContext, not a second DbContext.
builder.Services.AddScoped<IHeroStore>(sp => sp.GetRequiredService<HeroDbContext>());

var app = builder.Build();

// Serves files from wwwroot: css/site.css from Day 2, optional task D.
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
