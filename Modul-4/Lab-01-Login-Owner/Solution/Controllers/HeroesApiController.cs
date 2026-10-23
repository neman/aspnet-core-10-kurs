using HeroRegistry.Api;
using HeroRegistry.Hubs;
using HeroRegistry.Models;
using HeroRegistry.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HeroRegistry.Controllers;

[ApiController]
[Route("api/heroes")]
public sealed class HeroesApiController(IHeroStore store, IHubContext<HeroHub> hub) : ControllerBase
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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<HeroResponse>> Create(CreateHeroRequest request)
    {
        var hero = new Hero(
            User.Identity!.Name!,
            request.Codename,
            request.OriginStory ?? "",
            request.Race!.Value,
            request.Alignment!.Value);
        store.Add(hero);
        await store.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = hero.Id }, HeroResponse.From(hero));
    }

    // A broken rule throws a DomainException; DomainExceptionHandler turns it into 400 or 409.
    [Authorize]
    [HttpPost("{id:int}/register")]
    public async Task<ActionResult<HeroResponse>> Register(int id)
    {
        var hero = await FindAsync(id);
        if (hero is null)
            return NotFound();

        hero.Register();
        await store.SaveChangesAsync();

        // Only after the save, so no browser shows a hero that was not saved.
        await hub.Clients.All.SendAsync("HeroRegistered", new { hero.Id, hero.Codename });

        return HeroResponse.From(hero);
    }

    [Authorize]
    [HttpPost("{id:int}/powers/{powerId:int}")]
    public async Task<ActionResult<HeroResponse>> AddPower(int id, int powerId)
    {
        var hero = await FindAsync(id);
        var power = await store.Powers.FirstOrDefaultAsync(p => p.Id == powerId);
        if (hero is null || power is null)
            return NotFound();

        hero.AddPower(power);
        await store.SaveChangesAsync();

        return HeroResponse.From(hero);
    }

    [Authorize]
    [HttpPost("{id:int}/retire")]
    public async Task<ActionResult<HeroResponse>> Retire(int id)
    {
        var hero = await FindAsync(id);
        if (hero is null)
            return NotFound();

        hero.Retire();
        await store.SaveChangesAsync();

        await hub.Clients.All.SendAsync("HeroRetired", hero.Id);

        return HeroResponse.From(hero);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var hero = await FindAsync(id);
        if (hero is null)
            return NotFound();

        hero.EnsureCanBeDeleted();
        store.Remove(hero);
        await store.SaveChangesAsync();

        return NoContent();
    }

    private Task<Hero?> FindAsync(int id) => store.Heroes.FirstOrDefaultAsync(h => h.Id == id);
}
