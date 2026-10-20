using Microsoft.AspNetCore.Identity;

namespace Parko.Services;

public interface IAccountService
{
    Task<IdentityResult> RegisterAsync(string userName, string email, string password);

    Task<SignInResult> LoginAsync(string login, string password, bool rememberMe);

    Task LogoutAsync();

    Task<AccountClosureResult> CloseAccountAsync(string userId, string password);
}
