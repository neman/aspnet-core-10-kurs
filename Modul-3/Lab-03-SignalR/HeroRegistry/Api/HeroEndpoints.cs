using HeroRegistry.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HeroRegistry.Api;

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
            IHeroStore store) =>
        {
            var hero = await store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
            if (hero is null)
                return TypedResults.NotFound();

            hero.Register();
            await store.SaveChangesAsync();

            return TypedResults.Ok(HeroResponse.From(hero));
        });

        return heroes;
    }
}
