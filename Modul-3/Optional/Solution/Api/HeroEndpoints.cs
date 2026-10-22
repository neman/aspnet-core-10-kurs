using HeroRegistry.Hubs;
using HeroRegistry.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace HeroRegistry.Api;

// Optional D: the whole lifecycle from Lab 2, with the same SignalR messages as the controller.
public static class HeroEndpoints
{
    public static RouteGroupBuilder MapHeroEndpoints(this IEndpointRouteBuilder app)
    {
        var heroes = app.MapGroup("/api/minimal/heroes").WithTags("Heroes (Minimal API)");

        heroes.MapGet("/", async (IHeroStore store) =>
        {
            var registered = await store.Heroes.Registered().ToListAsync();
            return TypedResults.Ok(registered.Select(HeroResponse.From).ToList());
        });

        // The return type lists every possible result, so OpenAPI shows both 200 and 404.
        heroes.MapGet("/{id:int}", async Task<Results<Ok<HeroResponse>, NotFound>> (
            int id,
            IHeroStore store) =>
        {
            var hero = await store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
            return hero is null ? TypedResults.NotFound() : TypedResults.Ok(HeroResponse.From(hero));
        })
        .WithName("GetMinimalHero");

        heroes.MapPost("/", async (CreateHeroRequest request, IHeroStore store) =>
        {
            var hero = new Hero(
                request.Codename,
                request.OriginStory ?? "",
                request.Race!.Value,
                request.Alignment!.Value);
            store.Add(hero);
            await store.SaveChangesAsync();

            return TypedResults.CreatedAtRoute(
                HeroResponse.From(hero),
                "GetMinimalHero",
                new { id = hero.Id });
        })
        .AddEndpointFilter<ValidationFilter<CreateHeroRequest>>();

        heroes.MapPost("/{id:int}/register", async Task<Results<Ok<HeroResponse>, NotFound>> (
            int id,
            IHeroStore store,
            IHubContext<HeroHub> hub,
            IOptionsSnapshot<RegistrationRulesOptions> rules) =>
        {
            var hero = await store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
            if (hero is null)
                return TypedResults.NotFound();

            // Optional A: the phrases come from appsettings.json and are read per request.
            hero.Register(rules.Value.ForbiddenPhrases);
            await store.SaveChangesAsync();
            await hub.Clients.All.SendAsync("HeroRegistered", new { hero.Id, hero.Codename });

            return TypedResults.Ok(HeroResponse.From(hero));
        });

        heroes.MapPost("/{id:int}/powers/{powerId:int}", async Task<Results<Ok<HeroResponse>, NotFound>> (
            int id,
            int powerId,
            IHeroStore store) =>
        {
            var hero = await store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
            var power = await store.Powers.FirstOrDefaultAsync(p => p.Id == powerId);
            if (hero is null || power is null)
                return TypedResults.NotFound();

            hero.AddPower(power);
            await store.SaveChangesAsync();

            return TypedResults.Ok(HeroResponse.From(hero));
        });

        heroes.MapPost("/{id:int}/retire", async Task<Results<Ok<HeroResponse>, NotFound>> (
            int id,
            IHeroStore store,
            IHubContext<HeroHub> hub) =>
        {
            var hero = await store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
            if (hero is null)
                return TypedResults.NotFound();

            hero.Retire();
            await store.SaveChangesAsync();
            await hub.Clients.All.SendAsync("HeroRetired", hero.Id);

            return TypedResults.Ok(HeroResponse.From(hero));
        });

        heroes.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id,
            IHeroStore store) =>
        {
            var hero = await store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
            if (hero is null)
                return TypedResults.NotFound();

            hero.EnsureCanBeDeleted();
            store.Remove(hero);
            await store.SaveChangesAsync();

            return TypedResults.NoContent();
        });

        return heroes;
    }
}
