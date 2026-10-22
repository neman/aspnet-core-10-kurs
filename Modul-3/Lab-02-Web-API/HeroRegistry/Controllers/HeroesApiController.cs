using HeroRegistry.Api;
using HeroRegistry.Services;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

[ApiController]
[Route("api/heroes")]
public sealed class HeroesApiController(IHeroStore store) : ControllerBase
{
    [HttpGet]
    public async Task<List<HeroResponse>> GetRegistered()
    {
        var heroes = await store.Heroes.Registered().ToListAsync();
        return heroes.Select(HeroResponse.From).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HeroResponse>> GetById(int id)
    {
        var hero = await FindAsync(id);
        if (hero is null)
            return NotFound();

        return HeroResponse.From(hero);
    }

    [HttpPost]
    public async Task<ActionResult<HeroResponse>> Create(CreateHeroRequest request)
    {
        var hero = new Hero(
            request.Codename,
            request.OriginStory ?? "",
            request.Race!.Value,
            request.Alignment!.Value);
        store.Add(hero);
        await store.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = hero.Id }, HeroResponse.From(hero));
    }

    // A broken rule throws a DomainException; DomainExceptionHandler turns it into 400 or 409.
    [HttpPost("{id:int}/register")]
    public async Task<ActionResult<HeroResponse>> Register(int id)
    {
        var hero = await FindAsync(id);
        if (hero is null)
            return NotFound();

        hero.Register();
        await store.SaveChangesAsync();

        return HeroResponse.From(hero);
    }

    // TODO Task 2: POST api/heroes/{id}/powers/{powerId}: add an existing power; 404 if the hero or the power is missing.
    // TODO Task 3: POST api/heroes/{id}/retire, in the same shape as Register.
    // TODO Task 4: DELETE api/heroes/{id}: EnsureCanBeDeleted, Remove, save, 204 NoContent.

    private Task<Hero?> FindAsync(int id) => store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
}
