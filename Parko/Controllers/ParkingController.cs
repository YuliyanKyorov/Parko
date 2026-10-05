using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Parko.Data;
using Parko.Models;
using Parko.Services;
namespace Parko.Controllers;

public class ParkingController : Controller {
    private readonly ParkoDbContext _db; private readonly ParkingService _svc;
    public ParkingController(ParkoDbContext db, ParkingService svc) { _db = db; _svc = svc; }

    public IActionResult Index() => View(new CheckInVm());

    public async Task<IActionResult> Board() {
        var activeCars = await _db.Cars.Where(c => c.ReleaseTime == null).ToListAsync();
        var recentReleasedCars = await _db.Cars.Where(c => c.ReleaseTime != null)
            .OrderByDescending(c => c.Id).Take(50).ToListAsync();
        ViewBag.Cars = activeCars.Concat(recentReleasedCars).OrderByDescending(c => c.Id).ToList();
        return PartialView("_Board", await _db.ParkingSpaces.Include(s => s.Cars).OrderBy(s => s.Id).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Lookup(string plate) {
        var v = await _svc.FindVipAsync(plate);
        if (v?.Client == null) return Json(new { vip = false });
        var c = v.Client;
        return Json(new { vip = true, model = v.Model, color = v.Color, year = v.Year,
            owner = $"{c.FirstName} {c.LastName}", city = c.City, country = c.Country, phone = c.Phone,
            points = c.VipPoint?.Points, days = c.VipPoint?.ChargingDays });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInVm vm) {
        if (!ModelState.IsValid) { TempData["Err"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)); return RedirectToAction(nameof(Index)); }
        var (ok, msg) = await _svc.CheckInAsync(vm);
        TempData[ok ? "Ok" : "Err"] = msg;
        return RedirectToAction(nameof(Index));
    }

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
}
