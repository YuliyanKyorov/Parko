using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parko.Data;
using Parko.Models;
using Parko.Services;
namespace Parko.Controllers;

public class VipController : Controller {
    private readonly ParkoDbContext _db; private readonly ParkingService _svc;
    public VipController(ParkoDbContext db, ParkingService svc) { _db = db; _svc = svc; }

    public async Task<IActionResult> Index() {
        await _svc.ResetPointsAsync();
        return View(await _db.VipCarRecords.Include(v => v.Client!).ThenInclude(c => c.VipPoint).ToListAsync());
    }

    public IActionResult Create() => View(new VipCreateVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int clientId) {
        var client = await _db.Clients.FindAsync(clientId);
        if (client == null) {
            TempData["Err"] = "VIP client not found.";
            return RedirectToAction(nameof(Index));
        }

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP client deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VipCreateVm vm) {
        var plate = ParkingService.Norm(vm.Vehicle);
        if (await _db.VipCarRecords.AnyAsync(v => v.Vehicle == plate))
            ModelState.AddModelError(nameof(vm.Vehicle), "The number is already in the VIP database.");
        if (vm.Year > DateTime.Now.Year + 1) ModelState.AddModelError(nameof(vm.Year), "The year is in the future.");
        if (!ModelState.IsValid) return View(vm);
        var client = new Client { FirstName = vm.FirstName, LastName = vm.LastName, Address = vm.Address, City = vm.City,
            Country = vm.Country, Phone = vm.Phone, Email = vm.Email, VipPoint = new VipPoint() };
        client.VipCarRecords.Add(new VipCarRecord { Vehicle = plate, Model = vm.Model, Color = vm.Color, Year = vm.Year });
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
