using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
