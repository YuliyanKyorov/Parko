using Microsoft.AspNetCore.Identity;
using Parko.Models;

namespace Parko.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public Task<IdentityResult> RegisterAsync(string userName, string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName.Trim(),
            Email = email.Trim()
        };

        return _userManager.CreateAsync(user, password);
    }

    public async Task<SignInResult> LoginAsync(string login, string password, bool rememberMe)
    {
        var identifier = login.Trim();
        var user = await _userManager.FindByNameAsync(identifier);
        user ??= await _userManager.FindByEmailAsync(identifier);

        if (user is null)
        {
            return SignInResult.Failed;
        }

        return await _signInManager.PasswordSignInAsync(
            user,
            password,
            rememberMe,
            lockoutOnFailure: true);
    }

    public Task LogoutAsync() => _signInManager.SignOutAsync();

    public async Task<AccountClosureResult> CloseAccountAsync(string userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AccountClosureResult.NotFound;
        }

        if (await _userManager.IsInRoleAsync(user, IdentitySeeder.AdministratorRole))
        {
            return AccountClosureResult.Administrator;
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return AccountClosureResult.InvalidPassword;
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return AccountClosureResult.Failed;
        }

        await _signInManager.SignOutAsync();
        return AccountClosureResult.Closed;
    }
}

public enum AccountClosureResult
{
    Closed,
    NotFound,
    Administrator,
    InvalidPassword,
    Failed
}
