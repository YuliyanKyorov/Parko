using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parko.Services;

namespace Parko.Controllers;

[Authorize(Roles = IdentitySeeder.AdministratorRole)]
public class UsersController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _userManagementService.GetRegisteredUsersAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _userManagementService.DeleteUserAsync(id);
        TempData[result == UserDeletionResult.Deleted ? "Ok" : "Err"] = result switch
        {
            UserDeletionResult.Deleted => "User account deleted.",
            UserDeletionResult.NotFound => "User account not found.",
            UserDeletionResult.Administrator => "Administrator accounts cannot be deleted here.",
            _ => "The user account could not be deleted."
        };

        return RedirectToAction(nameof(Index));
    }
}
