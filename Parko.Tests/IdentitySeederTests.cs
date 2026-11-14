using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Parko.Models;
using Parko.Services;

namespace Parko.Tests;

public class IdentitySeederTests
{
    [Fact]
    public async Task EnsureAdministrator_creates_roles_accounts_and_assigns_default_roles()
    {
        await using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var regular = new ApplicationUser { UserName = "regular", Email = "regular@example.test" };
        Assert.True((await users.CreateAsync(regular, "password")).Succeeded);

        await IdentitySeeder.EnsureAdministratorAsync(users, roles, "admin", "adminpass");
        await IdentitySeeder.EnsureAdministratorAsync(users, roles, "admin", "adminpass");

        var administrator = await users.FindByNameAsync("admin");
        Assert.NotNull(administrator);
        Assert.True(await roles.RoleExistsAsync(IdentitySeeder.UserRole));
        Assert.True(await roles.RoleExistsAsync(IdentitySeeder.AdministratorRole));
        Assert.True(await users.IsInRoleAsync(administrator, IdentitySeeder.AdministratorRole));
        Assert.True(await users.IsInRoleAsync(regular, IdentitySeeder.UserRole));
    }

    [Fact]
    public async Task EnsureAdministrator_migrates_legacy_admin_role()
    {
        await using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.True((await roles.CreateAsync(new IdentityRole("Admin"))).Succeeded);
        var legacyAdministrator = new ApplicationUser { UserName = "legacy", Email = "legacy@example.test" };
        Assert.True((await users.CreateAsync(legacyAdministrator, "password")).Succeeded);
        Assert.True((await users.AddToRoleAsync(legacyAdministrator, "Admin")).Succeeded);

        await IdentitySeeder.EnsureAdministratorAsync(users, roles, "legacy", "password");

        Assert.True(await users.IsInRoleAsync(legacyAdministrator, IdentitySeeder.AdministratorRole));
        Assert.False(await users.IsInRoleAsync(legacyAdministrator, "Admin"));
        Assert.False(await roles.RoleExistsAsync("Admin"));
    }

    [Fact]
    public async Task EnsureAdministrator_rejects_an_existing_account_with_a_different_password()
    {
        await using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var existing = new ApplicationUser { UserName = "admin", Email = "admin@example.test" };
        Assert.True((await users.CreateAsync(existing, "old-password")).Succeeded);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            IdentitySeeder.EnsureAdministratorAsync(users, roles, "admin", "new-password"));
    }
}
