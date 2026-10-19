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
}
