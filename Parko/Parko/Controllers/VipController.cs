using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parko.Data;
using Parko.Models;
using Parko.Services;

namespace Parko.Controllers;

[Authorize(Roles = IdentitySeeder.AdministratorRole)]
public class VipController : Controller
{
    private readonly ParkoDbContext _db;
    private readonly ParkingService _parking;

    public VipController(ParkoDbContext db, ParkingService parking)
    {
        _db = db;
        _parking = parking;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _parking.ResetPointsAsync();
        var clients = await _db.Clients
            .Include(client => client.VipPoint)
            .Include(client => client.VipCarRecords)
            .OrderBy(client => client.LastName)
            .ThenBy(client => client.FirstName)
            .ToListAsync();

        return View(clients);
    }

    [HttpGet]
    public IActionResult Create() => View(new VipCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VipCreateViewModel input)
    {
        var plate = ParkingService.NormalizePlate(input.Vehicle);
        await AddDuplicatePlateErrorAsync(plate, nameof(input.Vehicle));

        if (input.Year > DateTime.Now.Year + 1)
        {
            ModelState.AddModelError(nameof(input.Year), "The year is in the future.");
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var client = new Client
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            Address = input.Address,
            City = input.City,
            Country = input.Country,
            Phone = input.Phone,
            Email = input.Email,
            VipPoint = new VipPoint()
        };

        client.VipCarRecords.Add(new VipCarRecord
        {
            Vehicle = plate,
            Model = input.Model,
            Color = input.Color,
            Year = input.Year
        });

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP client created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditClient(int id)
    {
        var client = await _db.Clients
            .Include(existingClient => existingClient.VipPoint)
            .FirstOrDefaultAsync(existingClient => existingClient.Id == id);
        if (client == null)
        {
            return NotFound();
        }

        return View(new VipClientEditViewModel
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Address = client.Address,
            City = client.City,
            Country = client.Country,
            Phone = client.Phone,
            Email = client.Email,
            Points = client.VipPoint?.Points ?? VipPoint.MonthlyPoints
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(VipClientEditViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var client = await _db.Clients
            .Include(existingClient => existingClient.VipPoint)
            .FirstOrDefaultAsync(existingClient => existingClient.Id == input.Id);
        if (client == null)
        {
            return NotFound();
        }

        client.FirstName = input.FirstName.Trim();
        client.LastName = input.LastName.Trim();
        client.Address = input.Address.Trim();
        client.City = input.City.Trim();
        client.Country = input.Country.Trim();
        client.Phone = input.Phone.Trim();
        client.Email = input.Email.Trim();

        if (client.VipPoint == null)
        {
            client.VipPoint = new VipPoint { Points = input.Points };
        }
        else
        {
            client.VipPoint.Points = input.Points;
        }

        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP client details and points updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null)
        {
            TempData["Err"] = "VIP client not found.";
            return RedirectToAction(nameof(Index));
        }

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP client and its VIP data deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CreateCar(int clientId)
    {
        if (!await _db.Clients.AnyAsync(client => client.Id == clientId))
        {
            return NotFound();
        }

        return View("Car", new VipCarRecordEditViewModel { ClientId = clientId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCar(VipCarRecordEditViewModel input)
    {
        await ValidateVipCarAsync(input);
        if (!ModelState.IsValid)
        {
            return View("Car", input);
        }

        _db.VipCarRecords.Add(new VipCarRecord
        {
            ClientId = input.ClientId,
            Vehicle = ParkingService.NormalizePlate(input.Vehicle),
            Model = input.Model.Trim(),
            Color = input.Color.Trim(),
            Year = input.Year
        });

        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP vehicle added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditCar(int id)
    {
        var vipCar = await _db.VipCarRecords.FindAsync(id);
        if (vipCar == null)
        {
            return NotFound();
        }

        return View("Car", new VipCarRecordEditViewModel
        {
            Id = vipCar.Id,
            ClientId = vipCar.ClientId,
            Vehicle = vipCar.Vehicle,
            Model = vipCar.Model,
            Color = vipCar.Color,
            Year = vipCar.Year
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCar(VipCarRecordEditViewModel input)
    {
        var vipCar = await _db.VipCarRecords.FindAsync(input.Id);
        if (vipCar == null)
        {
            return NotFound();
        }

        input.ClientId = vipCar.ClientId;
        await ValidateVipCarAsync(input, vipCar.Id);
        if (!ModelState.IsValid)
        {
            return View("Car", input);
        }

        vipCar.Vehicle = ParkingService.NormalizePlate(input.Vehicle);
        vipCar.Model = input.Model.Trim();
        vipCar.Color = input.Color.Trim();
        vipCar.Year = input.Year;

        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP vehicle details updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var vipCar = await _db.VipCarRecords.FindAsync(id);
        if (vipCar == null)
        {
            TempData["Err"] = "VIP vehicle not found.";
            return RedirectToAction(nameof(Index));
        }

        _db.VipCarRecords.Remove(vipCar);
        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP vehicle deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePoints(int clientId)
    {
        var vipPoints = await _db.VipPoints
            .FirstOrDefaultAsync(points => points.ClientId == clientId);
        if (vipPoints == null)
        {
            TempData["Err"] = "VIP points not found.";
            return RedirectToAction(nameof(Index));
        }

        _db.VipPoints.Remove(vipPoints);
        await _db.SaveChangesAsync();
        TempData["Ok"] = "VIP points deleted. They can be initialized again by editing the client.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateVipCarAsync(VipCarRecordEditViewModel input, int? currentId = null)
    {
        if (!await _db.Clients.AnyAsync(client => client.Id == input.ClientId))
        {
            ModelState.AddModelError(nameof(input.ClientId), "Select an existing VIP client.");
        }

        var plate = ParkingService.NormalizePlate(input.Vehicle);
        if (await _db.VipCarRecords.AnyAsync(vipCar =>
            vipCar.Vehicle == plate && vipCar.Id != currentId))
        {
            ModelState.AddModelError(nameof(input.Vehicle), "The number is already in the VIP database.");
        }

        if (input.Year > DateTime.Now.Year + 1)
        {
            ModelState.AddModelError(nameof(input.Year), "The year is in the future.");
        }
    }

    private async Task AddDuplicatePlateErrorAsync(string plate, string fieldName)
    {
        if (await _db.VipCarRecords.AnyAsync(vipCar => vipCar.Vehicle == plate))
        {
            ModelState.AddModelError(fieldName, "The number is already in the VIP database.");
        }
    }
}
