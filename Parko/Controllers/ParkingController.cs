using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Parko.Data;
using Parko.Models;
using Parko.Services;
namespace Parko.Controllers;

[Authorize]
public class ParkingController : Controller
{
    private readonly ParkoDbContext _db;
    private readonly ParkingService _parking;

    public ParkingController(ParkoDbContext db, ParkingService parking)
    {
        _db = db;
        _parking = parking;
    }

    public async Task<IActionResult> Index()
    {
        var parkingSpaces = await _db.ParkingSpaces
            .OrderBy(space => space.Id)
            .Select(space => space.Id)
            .ToListAsync();

        var occupiedSpaces = await _db.Cars
            .Where(car => car.ReleaseTime == null)
            .Select(car => car.ParkingSpaceId)
            .Distinct()
            .ToListAsync();

        var vipOccupiedSpaces = await _db.Cars
            .Where(car => car.ReleaseTime == null && car.IsVip)
            .Select(car => car.ParkingSpaceId)
            .Distinct()
            .ToListAsync();

        return View(new ParkingDashboardViewModel
        {
            ParkingSpaces = parkingSpaces,
            AvailableSpaces = parkingSpaces.Except(occupiedSpaces).ToList(),
            OccupiedSpaces = occupiedSpaces,
            VipOccupiedSpaces = vipOccupiedSpaces,
            SpaceMap = new ParkingSpaceMapViewModel
            {
                IsInteractive = true,
                Spaces = parkingSpaces.Select(spaceId => new ParkingSpaceStatusViewModel
                {
                    Id = spaceId,
                    IsOccupied = occupiedSpaces.Contains(spaceId),
                    IsVip = vipOccupiedSpaces.Contains(spaceId)
                }).ToList()
            },
            IsAdmin = User.IsInRole(IdentitySeeder.AdministratorRole)
        });
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    public async Task<IActionResult> Board()
    {
        var activeCars = await _db.Cars.Where(car => car.ReleaseTime == null).ToListAsync();
        var recentReleasedCars = await _db.Cars
            .Where(car => car.ReleaseTime != null)
            .OrderByDescending(car => car.Id)
            .Take(50)
            .ToListAsync();

        var spaces = await _db.ParkingSpaces
            .Include(space => space.Cars)
            .OrderBy(space => space.Id)
            .ToListAsync();

        return PartialView("_Board", new ParkingBoardViewModel
        {
            SpaceMap = new ParkingSpaceMapViewModel
            {
                Spaces = spaces.Select(space => new ParkingSpaceStatusViewModel
                {
                    Id = space.Id,
                    IsOccupied = space.Current != null,
                    IsVip = space.Current?.IsVip == true,
                    LicensePlate = space.Current?.Vehicle,
                    VehicleModel = space.Current?.Model
                }).ToList()
            },
            Cars = activeCars.Concat(recentReleasedCars).OrderByDescending(car => car.Id).ToList()
        });
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    [HttpGet]
    public async Task<IActionResult> Lookup(string plate)
    {
        var vipRecord = await _parking.FindVipAsync(plate);
        if (vipRecord?.Client == null)
        {
            return Json(new { vip = false });
        }

        var client = vipRecord.Client;
        return Json(new
        {
            vip = true,
            model = vipRecord.Model,
            color = vipRecord.Color,
            year = vipRecord.Year,
            owner = $"{client.FirstName} {client.LastName}",
            city = client.City,
            country = client.Country,
            phone = client.Phone,
            points = client.VipPoint?.Points,
            days = client.VipPoint?.ChargingDays
        });
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInViewModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Err"] = string.Join(" ", ModelState.Values
                .SelectMany(errorCollection => errorCollection.Errors)
                .Select(error => error.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        var (isSuccessful, message) = await _parking.CheckInAsync(input);
        TempData[isSuccessful ? "Ok" : "Err"] = message;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id, [Range(0, int.MaxValue)] int? simMinutes)
    {
        if (!ModelState.IsValid)
        {
            TempData["Err"] = "Minutes must be a non-negative whole number.";
            return RedirectToAction(nameof(Index));
        }

        var (isSuccessful, message) = await _parking.CheckOutAsync(id, simMinutes);
        TempData[isSuccessful ? "Ok" : "Err"] = message;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var result = await _parking.DeleteReleasedCarAsync(id);
        TempData[result == CarDeletionResult.Deleted ? "Ok" : "Err"] = result switch
        {
            CarDeletionResult.Deleted => "Parking record deleted.",
            CarDeletionResult.NotFound => "Parking record not found.",
            CarDeletionResult.CarStillParked => "Check out the vehicle before deleting its parking record.",
            _ => "The parking record could not be deleted."
        };

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(ParkCarViewModel input)
    {
        if (User.IsInRole(IdentitySeeder.AdministratorRole))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["Err"] = string.Join(" ", ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        var (isSuccessful, message) = await _parking.ParkCarAsync(input, userId);
        TempData[isSuccessful ? "Ok" : "Err"] = message;
        return RedirectToAction(nameof(Index));
    }
}
