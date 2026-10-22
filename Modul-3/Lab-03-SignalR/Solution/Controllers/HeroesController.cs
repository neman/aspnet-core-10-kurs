using HeroRegistry.Services;
using HeroRegistry.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

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

        store.Add(new Hero(
            input.Codename,
            input.OriginStory ?? "",
            input.Race!.Value,
            input.Alignment!.Value));
        store.SaveChanges();

        TempData["Message"] = $"Draft '{input.Codename}' saved. Only Registered heroes are public.";
        return RedirectToAction(nameof(Index));
    }
}
