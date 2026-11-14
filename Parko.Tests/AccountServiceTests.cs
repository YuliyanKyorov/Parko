using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Parko.Models;
using Parko.Services;

namespace Parko.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task Register_creates_trimmed_account_and_assigns_user_role()
    {
        var mocks = CreateMocks();
        mocks.UserManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "password"))
            .ReturnsAsync(IdentityResult.Success);
        mocks.UserManager
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), IdentitySeeder.UserRole))
            .ReturnsAsync(IdentityResult.Success);

        var result = await new AccountService(mocks.UserManager.Object, mocks.SignInManager.Object)
            .RegisterAsync(" user ", " person@example.test ", "password");

        Assert.True(result.Succeeded);
        mocks.UserManager.Verify(manager => manager.CreateAsync(
            It.Is<ApplicationUser>(user => user.UserName == "user" && user.Email == "person@example.test"),
            "password"), Times.Once);
        mocks.UserManager.Verify(manager => manager.AddToRoleAsync(
            It.IsAny<ApplicationUser>(), IdentitySeeder.UserRole), Times.Once);
    }

    [Fact]
    public async Task Register_rejects_missing_input_and_returns_create_failure()
    {
        var mocks = CreateMocks();
        var createFailure = IdentityResult.Failed(new IdentityError { Description = "Invalid account." });
        mocks.UserManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(createFailure);
        var service = new AccountService(mocks.UserManager.Object, mocks.SignInManager.Object);

        var missing = await service.RegisterAsync(" ", "person@example.test", "password");
        var rejected = await service.RegisterAsync("user", "person@example.test", "password");

        Assert.False(missing.Succeeded);
        Assert.Equal("InvalidInput", Assert.Single(missing.Errors).Code);
        Assert.Same(createFailure, rejected);
        mocks.UserManager.Verify(manager => manager.AddToRoleAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_removes_account_when_role_assignment_fails()
    {
        var mocks = CreateMocks();
        var roleFailure = IdentityResult.Failed(new IdentityError { Description = "Role missing." });
        mocks.UserManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mocks.UserManager
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), IdentitySeeder.UserRole))
            .ReturnsAsync(roleFailure);
        mocks.UserManager
            .Setup(manager => manager.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await new AccountService(mocks.UserManager.Object, mocks.SignInManager.Object)
            .RegisterAsync("user", "person@example.test", "password");

        Assert.Same(roleFailure, result);
        mocks.UserManager.Verify(manager => manager.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Register_surfaces_incomplete_cleanup_failure()
    {
        var mocks = CreateMocks();
        mocks.UserManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mocks.UserManager
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), IdentitySeeder.UserRole))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role failure." }));
        mocks.UserManager
            .Setup(manager => manager.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failure." }));

        var service = new AccountService(mocks.UserManager.Object, mocks.SignInManager.Object);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegisterAsync("user", "person@example.test", "password"));

        Assert.Contains("Delete failure", exception.Message);
    }

    [Fact]
    public async Task Login_uses_username_then_email_and_rejects_missing_accounts()
    {
        var mocks = CreateMocks();
        var user = new ApplicationUser { Id = "user" };
        mocks.UserManager.Setup(manager => manager.FindByNameAsync("person"))
            .ReturnsAsync((ApplicationUser?)null);
        mocks.UserManager.Setup(manager => manager.FindByEmailAsync("person"))
            .ReturnsAsync(user);
        mocks.SignInManager.Setup(manager => manager.PasswordSignInAsync(user, "password", true, true))
            .ReturnsAsync(SignInResult.Success);
        mocks.UserManager.Setup(manager => manager.FindByNameAsync("missing"))
            .ReturnsAsync((ApplicationUser?)null);
        mocks.UserManager.Setup(manager => manager.FindByEmailAsync("missing"))
            .ReturnsAsync((ApplicationUser?)null);
        var service = new AccountService(mocks.UserManager.Object, mocks.SignInManager.Object);

        Assert.True((await service.LoginAsync(" person ", "password", true)).Succeeded);
        Assert.False((await service.LoginAsync("missing", "password", false)).Succeeded);
        Assert.False((await service.LoginAsync("", "password", false)).Succeeded);
        mocks.SignInManager.Verify(manager => manager.PasswordSignInAsync(user, "password", true, true), Times.Once);
    }

    [Fact]
    public async Task CloseAccount_protects_admin_and_checks_password_and_delete_result()
    {
        var mocks = CreateMocks();
        var user = new ApplicationUser { Id = "user" };
        mocks.UserManager.Setup(manager => manager.FindByIdAsync("missing"))
            .ReturnsAsync((ApplicationUser?)null);
        mocks.UserManager.Setup(manager => manager.FindByIdAsync("admin"))
            .ReturnsAsync(user);
        mocks.UserManager.Setup(manager => manager.FindByIdAsync("user"))
            .ReturnsAsync(user);
        mocks.UserManager.Setup(manager => manager.IsInRoleAsync(user, IdentitySeeder.AdministratorRole))
            .ReturnsAsync(false);
        mocks.UserManager.Setup(manager => manager.IsInRoleAsync(user, IdentitySeeder.AdministratorRole))
            .ReturnsAsync(false);
        mocks.UserManager.Setup(manager => manager.CheckPasswordAsync(user, "wrong"))
            .ReturnsAsync(false);
        mocks.UserManager.Setup(manager => manager.CheckPasswordAsync(user, "correct"))
            .ReturnsAsync(true);
        mocks.UserManager.Setup(manager => manager.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError()));
        var service = new AccountService(mocks.UserManager.Object, mocks.SignInManager.Object);

        Assert.Equal(AccountClosureResult.NotFound, await service.CloseAccountAsync("", "password"));
        Assert.Equal(AccountClosureResult.NotFound, await service.CloseAccountAsync("missing", "password"));
        Assert.Equal(AccountClosureResult.InvalidPassword, await service.CloseAccountAsync("user", ""));
        mocks.UserManager.Setup(manager => manager.IsInRoleAsync(user, IdentitySeeder.AdministratorRole))
            .ReturnsAsync(true);
        Assert.Equal(AccountClosureResult.Administrator, await service.CloseAccountAsync("admin", "password"));
        mocks.UserManager.Setup(manager => manager.IsInRoleAsync(user, IdentitySeeder.AdministratorRole))
            .ReturnsAsync(false);
        Assert.Equal(AccountClosureResult.InvalidPassword, await service.CloseAccountAsync("user", "wrong"));
        Assert.Equal(AccountClosureResult.Failed, await service.CloseAccountAsync("user", "correct"));

        mocks.UserManager.Setup(manager => manager.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        Assert.Equal(AccountClosureResult.Closed, await service.CloseAccountAsync("user", "correct"));
        mocks.SignInManager.Verify(manager => manager.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task Logout_signs_out()
    {
        var mocks = CreateMocks();
        mocks.SignInManager.Setup(manager => manager.SignOutAsync()).Returns(Task.CompletedTask);

        await new AccountService(mocks.UserManager.Object, mocks.SignInManager.Object).LogoutAsync();

        mocks.SignInManager.Verify(manager => manager.SignOutAsync(), Times.Once);
    }

    private static (
        Mock<UserManager<ApplicationUser>> UserManager,
        Mock<SignInManager<ApplicationUser>> SignInManager) CreateMocks()
    {
        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            new HttpContextAccessor(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<ApplicationUser>>.Instance,
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        return (userManager, signInManager);
    }
}
