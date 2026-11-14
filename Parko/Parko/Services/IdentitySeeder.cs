using Microsoft.AspNetCore.Identity;
using Parko.Models;

namespace Parko.Services;

public static class IdentitySeeder
{
    public const string AdministratorRole = "Admin";

    public static async Task EnsureAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        string userName,
        string password)
    {
        if (!await roleManager.RoleExistsAsync(AdministratorRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdministratorRole));
            EnsureSucceeded(roleResult, "creating the administrator role");
        }

        var administrator = await userManager.FindByNameAsync(userName);
        if (administrator is null)
        {
            administrator = new ApplicationUser
            {
                UserName = userName,
                Email = "admin@parko.local",
                EmailConfirmed = true
            };
            var userResult = await userManager.CreateAsync(administrator, password);
            EnsureSucceeded(userResult, "creating the configured administrator account");
        }
        var isAdministrator = await userManager.IsInRoleAsync(administrator, AdministratorRole);
        if (!isAdministrator && !await userManager.CheckPasswordAsync(administrator, password))
        {
            throw new InvalidOperationException(
                $"The configured administrator username '{userName}' already belongs to an account with a different password. " +
                "Set AdminAccount:UserName to a new username or configure the matching administrator password.");
        }

        if (!isAdministrator)
        {
            var roleResult = await userManager.AddToRoleAsync(administrator, AdministratorRole);
            EnsureSucceeded(roleResult, "assigning the administrator role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Identity failed while {operation}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
