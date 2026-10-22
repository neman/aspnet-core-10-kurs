using HeroRegistry.Services;
using HeroRegistry.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

    // Optional B: the same lifecycle from MVC pages. The domain throws the same
    // exceptions as for the API; here they become a message on the page.
    [HttpGet]
    public IActionResult Drafts()
    {
        var drafts = store.Heroes.Drafts()
            .Select(h => new HeroListItemViewModel(h.Id, h.Codename))
            .ToList();
        return View(drafts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(
        int id,
        [FromServices] IOptionsSnapshot<RegistrationRulesOptions> rules)
    {
        var hero = store.Heroes.FirstOrDefault(h => h.Id == id);
        if (hero is null)
            return NotFound();

        try
        {
            hero.Register(rules.Value.ForbiddenPhrases);
            store.SaveChanges();
            TempData["Message"] = $"'{hero.Codename}' is now Registered.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            TempData["Message"] = ex.Message;
            return RedirectToAction(nameof(Drafts));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Retire(int id)
    {
        var hero = store.Heroes.FirstOrDefault(h => h.Id == id);
        if (hero is null)
            return NotFound();

        try
        {
            hero.Retire();
            store.SaveChanges();
            TempData["Message"] = $"'{hero.Codename}' is now Retired.";
        }
        catch (DomainException ex)
        {
            TempData["Message"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var hero = store.Heroes.FirstOrDefault(h => h.Id == id);
        if (hero is null)
            return NotFound();

        try
        {
            hero.EnsureCanBeDeleted();
            store.Remove(hero);
            store.SaveChanges();
            TempData["Message"] = $"'{hero.Codename}' was deleted.";
        }
        catch (DomainException ex)
        {
            TempData["Message"] = ex.Message;
        }
        return RedirectToAction(nameof(Drafts));
    }
}
