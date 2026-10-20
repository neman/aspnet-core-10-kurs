using HeroRegistry.Models;
using HeroRegistry.Services;
using HeroRegistry.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

public sealed class HeroesController(IHeroStore store) : Controller
{
    // Bonus 3: search comes from the query string (?search=aur), bound by parameter name.
    [HttpGet]
    public IActionResult Index(string? search)
    {
        var heroes = store.Heroes.Registered()
            .Where(h => string.IsNullOrWhiteSpace(search) ||
                h.Codename.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(h => new HeroListItemViewModel(h.Id, h.Codename))
            .ToList();
        return View(heroes);
    }

    // Bonus 1: attribute route. /heroes/1 works; /Heroes/Details/1 is now 404.
    [HttpGet("heroes/{id:int:min(1)}")]
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
}
