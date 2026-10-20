using HeroRegistry.Models;
using HeroRegistry.Services;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.ViewComponents;

public sealed class HeroCountViewComponent(IHeroStore store) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        int count = store.Heroes.Registered().Count();
        return View(count);
    }
}
