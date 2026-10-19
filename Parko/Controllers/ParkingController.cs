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
public class ParkingController : Controller {
    private readonly ParkoDbContext _db; private readonly ParkingService _svc;
    public ParkingController(ParkoDbContext db, ParkingService svc) { _db = db; _svc = svc; }

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
            IsAdmin = User.IsInRole(IdentitySeeder.AdministratorRole)
        });
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    public async Task<IActionResult> Board() {
        var activeCars = await _db.Cars.Where(c => c.ReleaseTime == null).ToListAsync();
        var recentReleasedCars = await _db.Cars.Where(c => c.ReleaseTime != null)
            .OrderByDescending(c => c.Id).Take(50).ToListAsync();
        ViewBag.Cars = activeCars.Concat(recentReleasedCars).OrderByDescending(c => c.Id).ToList();
        return PartialView("_Board", await _db.ParkingSpaces.Include(s => s.Cars).OrderBy(s => s.Id).ToListAsync());
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    [HttpGet]
    public async Task<IActionResult> Lookup(string plate) {
        var v = await _svc.FindVipAsync(plate);
        if (v?.Client == null) return Json(new { vip = false });
        var c = v.Client;
        return Json(new { vip = true, model = v.Model, color = v.Color, year = v.Year,
            owner = $"{c.FirstName} {c.LastName}", city = c.City, country = c.Country, phone = c.Phone,
            points = c.VipPoint?.Points, days = c.VipPoint?.ChargingDays });
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInVm vm) {
        if (!ModelState.IsValid) { TempData["Err"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)); return RedirectToAction(nameof(Index)); }
        var (ok, msg) = await _svc.CheckInAsync(vm);
        TempData[ok ? "Ok" : "Err"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentitySeeder.AdministratorRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id, [Range(0, int.MaxValue)] int? simMinutes) {
        if (!ModelState.IsValid) {
            TempData["Err"] = "Minutes must be a non-negative whole number.";
            return RedirectToAction(nameof(Index));
        }
        var (ok, msg) = await _svc.CheckOutAsync(id, simMinutes);
        TempData[ok ? "Ok" : "Err"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(ParkCarViewModel vm)
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

        var (ok, message) = await _svc.ParkCarAsync(vm, userId);
        TempData[ok ? "Ok" : "Err"] = message;
        return RedirectToAction(nameof(Index));
    }
}
