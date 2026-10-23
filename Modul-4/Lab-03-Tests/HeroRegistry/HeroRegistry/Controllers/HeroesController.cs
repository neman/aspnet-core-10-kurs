using HeroRegistry.Models;
using HeroRegistry.Services;
using HeroRegistry.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

public sealed class HeroesController(IHeroStore store, IAuthorizationService authorization) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var heroes = store.Heroes.Registered()
            .Select(h => new HeroListItemViewModel(h.Id, h.Codename))
            .ToList();
        return View(heroes);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var hero = store.Heroes.Registered().FirstOrDefault(h => h.Id == id);
        if (hero is null)
            return NotFound();

        return View(new HeroDetailsViewModel(
            hero.Id, hero.Codename, hero.OriginStory,
            hero.Race.ToString(), hero.Alignment.ToString(),
            hero.Powers.Select(p => p.Name).ToList()));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Drafts()
    {
        var drafts = store.Heroes.OwnedBy(User.Identity!.Name!).Drafts()
            .Select(h => new HeroListItemViewModel(h.Id, h.Codename))
            .ToList();
        return View(drafts);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create() => View(new CreateHeroInputModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateHeroInputModel input)
    {
        if (!ModelState.IsValid)
            return View(input);

        // The owner comes from the cookie, never from the form.
        store.Add(new Hero(
            User.Identity!.Name!,
            input.Codename,
            input.OriginStory ?? "",
            input.Race!.Value,
            input.Alignment!.Value));
        store.SaveChanges();

        TempData["Message"] = $"Draft '{input.Codename}' saved. Only Registered heroes are public.";
        return RedirectToAction(nameof(Drafts));
    }

    // The Drafts page shows only my heroes, but a POST can carry any id:
    // the server checks the owner again.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var hero = store.Heroes.FirstOrDefault(h => h.Id == id);
        if (hero is null)
            return NotFound();

        var decision = await authorization.AuthorizeAsync(User, hero, "HeroOwner");
        if (!decision.Succeeded)
            return Forbid(); // cookie scheme: redirect to /Account/Denied

        hero.EnsureCanBeDeleted();
        store.Remove(hero);
        store.SaveChanges();

        TempData["Message"] = $"Draft '{hero.Codename}' deleted.";
        return RedirectToAction(nameof(Drafts));
    }
}
