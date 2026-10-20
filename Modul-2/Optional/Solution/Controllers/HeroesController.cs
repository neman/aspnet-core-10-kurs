using HeroRegistry.Filters;
using HeroRegistry.Models;
using HeroRegistry.Services;
using HeroRegistry.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

// Optional task E: the filter runs only around the actions of this controller.
[ServiceFilter(typeof(ActionTimingFilter))]
public sealed class HeroesController(IHeroStore store) : Controller
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

    [HttpGet]
    public IActionResult Create() => View(new CreateHeroInputModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateHeroInputModel input)
    {
        if (!ModelState.IsValid)
            return View(input);

        store.Add(new Hero(input.Codename, input.OriginStory ?? "",
            input.Race!.Value, input.Alignment!.Value));
        store.SaveChanges();

        TempData["Message"] = $"Draft '{input.Codename}' saved. Only Registered heroes are public.";
        return RedirectToAction(nameof(Index));
    }

    // Optional task A: list and edit Drafts.
    [HttpGet]
    public IActionResult Drafts()
    {
        var drafts = store.Heroes.Drafts()
            .Select(h => new HeroListItemViewModel(h.Id, h.Codename))
            .ToList();
        return View(drafts);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var hero = store.Heroes.Drafts().FirstOrDefault(h => h.Id == id);
        if (hero is null)
            return NotFound();

        return View(new CreateHeroInputModel
        {
            Codename = hero.Codename,
            OriginStory = hero.OriginStory,
            Race = hero.Race,
            Alignment = hero.Alignment
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, CreateHeroInputModel input)
    {
        var hero = store.Heroes.Drafts().FirstOrDefault(h => h.Id == id);
        if (hero is null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(input);

        hero.UpdateDraft(input.Codename, input.OriginStory ?? "",
            input.Race!.Value, input.Alignment!.Value);
        store.SaveChanges();

        TempData["Message"] = $"Draft '{input.Codename}' updated.";
        return RedirectToAction(nameof(Drafts));
    }
}
