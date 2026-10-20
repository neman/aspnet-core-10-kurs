using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    // Bonus 4: re-executed by UseStatusCodePagesWithReExecute; the original status code stays.
    public IActionResult Status(int id) => View(id);
}
