using HeroRegistry.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// TODO Task 2: Delete the two in-memory registrations below and, after AddDbContext,
// register IHeroStore as HeroDbContext (code in README).
builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddScoped<IHeroStore, InMemoryHeroStore>();

builder.Services.AddDbContext<HeroDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("HeroRegistry"))
    .UseSeeding((context, _) => HeroSeed.Seed(context)));

var app = builder.Build();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
