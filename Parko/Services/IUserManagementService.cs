using Parko.Models;

namespace Parko.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<RegisteredUserViewModel>> GetRegisteredUsersAsync();

    Task<UserDeletionResult> DeleteUserAsync(string userId);
}

public enum UserDeletionResult
{
    Deleted,
    NotFound,
    Administrator,
    Failed
}
