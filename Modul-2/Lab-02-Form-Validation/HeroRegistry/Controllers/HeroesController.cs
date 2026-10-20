using HeroRegistry.Models;
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

    // TODO Lab 02, Task 1: Create ViewModels/CreateHeroInputModel (Codename, OriginStory, Race, Alignment; no Id, no Status).
    // TODO Lab 02, Task 2: Add [HttpGet] Create() that returns the view with an empty input model.
    // TODO Lab 02, Task 3: Add [HttpPost] + [ValidateAntiForgeryToken] Create(CreateHeroInputModel input):
    //   invalid -> the same view with the same model; valid -> new Hero(...), Add, SaveChanges, TempData, redirect.
}
