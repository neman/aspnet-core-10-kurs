using System.Security.Claims;
using HeroRegistry.Security;
using HeroRegistry.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeroRegistry.Controllers;

public sealed class AccountController(CourseUsers users) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new LoginInputModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel input)
    {
        if (!ModelState.IsValid)
            return View(input);

        var user = users.Find(input.Name, input.Password);
        if (user is null)
        {
            ModelState.AddModelError("", "Unknown user or wrong password.");
            return View(input);
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, user.Name) };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

        // Only a local URL: "?ReturnUrl=https://evil.example" must not become an open redirect.
        return Redirect(Url.IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl : "/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // Authenticated, but not allowed: [Authorize] sends the browser here (403 for the API).
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Denied() => View();
}
