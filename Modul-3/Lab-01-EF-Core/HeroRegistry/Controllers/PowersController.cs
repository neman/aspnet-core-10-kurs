using HeroRegistry.Models;
using HeroRegistry.Services;
using HeroRegistry.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

public sealed class PowersController(IHeroStore store) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var powers = store.Powers.OrderBy(p => p.Name)
            .Select(p => new PowerListItemViewModel(p.Id, p.Name))
            .ToList();
        return View(powers);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreatePowerInputModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreatePowerInputModel input)
    {
        if (!ModelState.IsValid)
            return View(input);

        store.Add(new Power(input.Name));
        store.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}
