using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parko.Models;

namespace Parko.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<RegisteredUserViewModel>> GetRegisteredUsersAsync()
    {
        var administrators = await _userManager.GetUsersInRoleAsync(IdentitySeeder.AdministratorRole);
        var administratorIds = administrators.Select(user => user.Id).ToArray();

        return await _userManager.Users
            .Where(user => !administratorIds.Contains(user.Id))
            .OrderBy(user => user.UserName)
            .Select(user => new RegisteredUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email
            })
            .ToListAsync();
    }

    public async Task<UserDeletionResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return UserDeletionResult.NotFound;
        }

        if (await _userManager.IsInRoleAsync(user, IdentitySeeder.AdministratorRole))
        {
            return UserDeletionResult.Administrator;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded ? UserDeletionResult.Deleted : UserDeletionResult.Failed;
    }
}
