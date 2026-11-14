using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parko.Models;

namespace Parko.Services;

public static class IdentitySeeder
{
    public const string UserRole = "User";
    public const string AdministratorRole = "Administrator";
    private const string LegacyAdministratorRole = "Admin";

    public static async Task EnsureAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        string userName,
        string password)
    {
        await EnsureRoleExistsAsync(roleManager, UserRole);
        await EnsureRoleExistsAsync(roleManager, AdministratorRole);
        await MigrateLegacyAdministratorRoleAsync(userManager, roleManager);

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

        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            if (await userManager.IsInRoleAsync(user, AdministratorRole) ||
                await userManager.IsInRoleAsync(user, UserRole))
            {
                continue;
            }

            var roleResult = await userManager.AddToRoleAsync(user, UserRole);
            EnsureSucceeded(roleResult, "assigning the user role");
        }
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        EnsureSucceeded(roleResult, $"creating the {roleName} role");
    }

    private static async Task MigrateLegacyAdministratorRoleAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var legacyRole = await roleManager.FindByNameAsync(LegacyAdministratorRole);
        if (legacyRole is null)
        {
            return;
        }

        var legacyAdministrators = await userManager.GetUsersInRoleAsync(LegacyAdministratorRole);
        foreach (var user in legacyAdministrators)
        {
            if (!await userManager.IsInRoleAsync(user, AdministratorRole))
            {
                var addResult = await userManager.AddToRoleAsync(user, AdministratorRole);
                EnsureSucceeded(addResult, "migrating an administrator to the new role");
            }

            var removeResult = await userManager.RemoveFromRoleAsync(user, LegacyAdministratorRole);
            EnsureSucceeded(removeResult, "removing the legacy administrator role");
        }

        var deleteResult = await roleManager.DeleteAsync(legacyRole);
        EnsureSucceeded(deleteResult, "deleting the legacy administrator role");
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
