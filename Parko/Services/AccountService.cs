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

    public async Task<IdentityResult> RegisterAsync(string userName, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidInput",
                Description = "Username, email, and password are required."
            });
        }

        var user = new ApplicationUser
        {
            UserName = userName.Trim(),
            Email = email.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return createResult;
        }

        var roleResult = await _userManager.AddToRoleAsync(user, IdentitySeeder.UserRole);
        if (roleResult.Succeeded)
        {
            return IdentityResult.Success;
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not assign the user role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}. " +
                $"Could not remove the incomplete account: {string.Join("; ", deleteResult.Errors.Select(error => error.Description))}.");
        }

        return roleResult;
    }

    public async Task<SignInResult> LoginAsync(string login, string password, bool rememberMe)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return SignInResult.Failed;
        }

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
        if (string.IsNullOrWhiteSpace(userId))
        {
            return AccountClosureResult.NotFound;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AccountClosureResult.InvalidPassword;
        }

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
