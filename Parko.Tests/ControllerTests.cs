using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Parko.Areas.Admin.Controllers;
using Parko.Controllers;
using Parko.Models;
using Parko.Services;

namespace Parko.Tests;

public class ControllerTests
{
    [Fact]
    public async Task Parking_check_out_redirects_with_validation_error()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var controller = new ParkingController(
            database.Context,
            new ParkingService(database.Context));
        controller.TempData = new TempDataDictionary(
            new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
        controller.ModelState.AddModelError("simMinutes", "Invalid duration.");

        var result = await controller.CheckOut(1, -1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Minutes must be a non-negative whole number.", controller.TempData["Err"]);
    }

    [Fact]
    public async Task Parking_index_returns_consistent_available_and_occupied_spaces()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        database.Context.Cars.Add(new Car
        {
            Vehicle = "AB123",
            Model = "Test car",
            EntryTime = DateTime.Now,
            ParkingSpaceId = 1,
            IsVip = true
        });
        await database.Context.SaveChangesAsync();
        var controller = new ParkingController(database.Context, new ParkingService(database.Context));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };

        var result = Assert.IsType<ViewResult>(await controller.Index());
        var model = Assert.IsType<ParkingDashboardViewModel>(result.Model);

        Assert.Equal(100, model.ParkingSpaces.Count);
        Assert.Equal(99, model.AvailableSpaces.Count);
        Assert.Contains(1, model.OccupiedSpaces);
        Assert.Contains(1, model.VipOccupiedSpaces);
        Assert.True(model.SpaceMap.Spaces.Single(space => space.Id == 1).IsVip);
    }

    [Fact]
    public async Task Parking_admin_board_includes_details_for_occupied_spaces_only()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        database.Context.Cars.Add(new Car
        {
            Vehicle = "AB123",
            Model = "Test sedan",
            EntryTime = DateTime.Now,
            ParkingSpaceId = 1
        });
        await database.Context.SaveChangesAsync();
        var controller = new ParkingController(database.Context, new ParkingService(database.Context));

        var result = Assert.IsType<PartialViewResult>(await controller.Board());
        var model = Assert.IsType<ParkingBoardViewModel>(result.Model);
        var occupiedSpace = model.SpaceMap.Spaces.Single(space => space.Id == 1);
        var availableSpace = model.SpaceMap.Spaces.Single(space => space.Id == 2);

        Assert.Equal("AB123", occupiedSpace.LicensePlate);
        Assert.Equal("Test sedan", occupiedSpace.VehicleModel);
        Assert.Null(availableSpace.LicensePlate);
        Assert.Null(availableSpace.VehicleModel);
    }

    [Fact]
    public void Home_index_displays_homepage_for_authenticated_users()
    {
        var controller = new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(
                        new System.Security.Claims.ClaimsIdentity(
                            [new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.Name,
                                "test-user")],
                            "test"))
                }
            }
        };

        var result = Assert.IsType<ViewResult>(controller.Index());

        Assert.IsType<LoginViewModel>(result.Model);
    }

    [Fact]
    public async Task Account_login_renders_view_for_invalid_model()
    {
        var controller = new AccountController(Mock.Of<IAccountService>());
        controller.ModelState.AddModelError(nameof(LoginViewModel.Login), "Required.");

        var result = await controller.Login(new LoginViewModel());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Vip_create_car_returns_not_found_for_unknown_client()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var controller = new VipController(database.Context, new ParkingService(database.Context));

        var result = await controller.CreateCar(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Theory]
    [InlineData(404, "NotFound")]
    [InlineData(500, "InternalServerError")]
    public void Error_controller_selects_custom_view_and_preserves_status_code(int statusCode, string expectedView)
    {
        var controller = new ErrorController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            }
        };

        var result = Assert.IsType<ViewResult>(controller.Index(statusCode));

        Assert.Equal(expectedView, result.ViewName);
        Assert.Equal(statusCode, controller.Response.StatusCode);
    }
}
