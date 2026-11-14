using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Parko.Models;
using Parko.Services;

namespace Parko.Tests;

public class UserManagementServiceTests
{
    [Fact]
    public async Task GetRegisteredUsers_returns_sorted_non_administrator_accounts()
    {
        await using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var administrator = await CreateUserAsync(users, "z-admin", "admin@example.test");
        var regular = await CreateUserAsync(users, "z-user", "z@example.test");
        var first = await CreateUserAsync(users, "a-user", "a@example.test");
        Assert.True((await roles.CreateAsync(new IdentityRole(IdentitySeeder.AdministratorRole))).Succeeded);
        Assert.True((await users.AddToRoleAsync(administrator, IdentitySeeder.AdministratorRole)).Succeeded);

        var result = await new UserManagementService(users).GetRegisteredUsersAsync();

        Assert.Equal(new[] { "a-user", "z-user" }, result.Select(user => user.UserName));
        Assert.DoesNotContain(result, user => user.Id == administrator.Id);
        Assert.Equal("a@example.test", result[0].Email);
        Assert.NotEmpty(first.Id);
        Assert.NotEmpty(regular.Id);
    }

    [Fact]
    public async Task DeleteUser_handles_missing_administrator_and_regular_accounts()
    {
        await using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var administrator = await CreateUserAsync(users, "admin", "admin@example.test");
        var regular = await CreateUserAsync(users, "user", "user@example.test");
        Assert.True((await roles.CreateAsync(new IdentityRole(IdentitySeeder.AdministratorRole))).Succeeded);
        Assert.True((await users.AddToRoleAsync(administrator, IdentitySeeder.AdministratorRole)).Succeeded);
        var service = new UserManagementService(users);

        Assert.Equal(UserDeletionResult.NotFound, await service.DeleteUserAsync(""));
        Assert.Equal(UserDeletionResult.NotFound, await service.DeleteUserAsync("missing"));
        Assert.Equal(UserDeletionResult.Administrator, await service.DeleteUserAsync(administrator.Id));
        Assert.Equal(UserDeletionResult.Deleted, await service.DeleteUserAsync(regular.Id));
        Assert.Null(await users.FindByIdAsync(regular.Id));
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> users,
        string userName,
        string email)
    {
        var user = new ApplicationUser { UserName = userName, Email = email };
        var result = await users.CreateAsync(user, "password");
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        return user;
    }
}
