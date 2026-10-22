using HeroRegistry.Api;
using HeroRegistry.Data;
using HeroRegistry.Hubs;
using HeroRegistry.Security;
using HeroRegistry.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<HeroDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("HeroRegistry"))
    .UseSeeding((context, _) => HeroSeed.Seed(context)));

// The same instance as HeroDbContext, not a second DbContext.
builder.Services.AddScoped<IHeroStore>(sp => sp.GetRequiredService<HeroDbContext>());

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddOpenApi();

builder.Services.AddSignalR();

builder.Services.AddSingleton<CourseUsers>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        // An anonymous API call gets 401 from Bearer, not a redirect to the login page.
        options.ForwardDefaultSelector = context =>
            context.Request.Path.StartsWithSegments("/api")
                ? JwtBearerDefaults.AuthenticationScheme
                : null;
    })
    .AddJwtBearer(); // settings come from "Authentication:Schemes:Bearer" (dotnet user-jwts)
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi(); // GET /openapi/v1.json
app.MapHeroEndpoints();
app.MapHub<HeroHub>("/hubs/heroes");

// Serves files from wwwroot: css/site.css from Day 2, optional task D.
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
