using System.Net.Http.Json;
using System.Security.Claims;
using Dormio.Web.Models;
using Dormio.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dormio.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ApiClient api, ILogger<AccountController> logger)
    {
        _api = api;
        _logger = logger;
    }

    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(new LoginVm()); }
    [HttpGet] public IActionResult Register() => View(new RegisterVm());
    [HttpGet] public IActionResult AccessDenied() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);
        var res = await _api.PostAsync("api/auth/login", vm);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }
        await SignInAsync((await res.Content.ReadFromJsonAsync<AuthResponseVm>())!);
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var res = await _api.PostAsync("api/auth/register", vm);
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogWarning("Register failed: {Body}", await res.Content.ReadAsStringAsync());
            ModelState.AddModelError(string.Empty, "Registration failed. The email may already be in use or the password is too weak.");
            return View(vm);
        }
        await SignInAsync((await res.Content.ReadFromJsonAsync<AuthResponseVm>())!);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInAsync(AuthResponseVm auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.UserId.ToString()),
            new(ClaimTypes.Name, auth.FullName),
            new(ClaimTypes.Email, auth.Email),
            new(ApiClient.TokenClaim, auth.Token)
        };
        claims.AddRange(auth.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { ExpiresUtc = auth.ExpiresAt, IsPersistent = true });
    }
}
