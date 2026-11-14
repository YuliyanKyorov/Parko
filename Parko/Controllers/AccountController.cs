using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Parko.Models;
using Parko.Services;

namespace Parko.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.LoginAsync(model.Login, model.Password, model.RememberMe);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(
            string.Empty,
            result.IsLockedOut
                ? "Your account is temporarily locked after failed sign-in attempts. Please try again later."
                : "Invalid username/email or password.");
        return View(model);
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.RegisterAsync(model.UserName, model.Email, model.Password);
        if (result.Succeeded)
        {
            TempData["Success"] = "Registration successful. Sign in with your new account.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet, Authorize]
    public IActionResult CloseAccount()
    {
        return View(new CloseAccountViewModel());
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseAccount(CloseAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        var result = await _accountService.CloseAccountAsync(userId, model.Password);
        if (result == AccountClosureResult.Closed)
        {
            TempData["Success"] = "Your account has been closed.";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, result switch
        {
            AccountClosureResult.InvalidPassword => "The password is incorrect.",
            AccountClosureResult.Administrator => "Administrator accounts cannot be closed from this page.",
            AccountClosureResult.NotFound => "Your account could not be found.",
            _ => "Your account could not be closed."
        });
        return View(model);
    }
}
