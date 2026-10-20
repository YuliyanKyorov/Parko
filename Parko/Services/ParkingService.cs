using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Parko.Data;
using Parko.Models;
namespace Parko.Services;

public class ParkingService
{
    private readonly ParkoDbContext _db;
    public ParkingService(ParkoDbContext db) => _db = db;

    public static string Norm(string? p) => Regex.Replace(p ?? "", @"[\s-]", "").ToUpperInvariant();

    // Standard rates: first 15 minutes free; up to 6 hours €1.20/hour; 6–12 hours €1/hour; over 12 hours €12/day.
    public static decimal Price(double min) =>
        min <= 15 ? 0m :
        min <= 360 ? (decimal)Math.Ceiling(min / 60) * 1.20m :
        min <= 720 ? (decimal)Math.Ceiling(min / 60) * 1.00m :
        (decimal)Math.Ceiling(min / 1440) * 12m;

    // VIP points reset to 40 on the first day of each month; unused points expire.
    public async Task ResetPointsAsync()
    {
        var n = DateTime.Now;
        var old = await _db.VipPoints.Where(p => p.LastReset.Year != n.Year || p.LastReset.Month != n.Month).ToListAsync();
        foreach (var p in old) { p.Points = VipPoint.Monthly; p.LastReset = n; }
        if (old.Count > 0) await _db.SaveChangesAsync();
    }

    public async Task<VipCarRecord?> FindVipAsync(string plate)
    {
        await ResetPointsAsync();
        var v = Norm(plate);
        return await _db.VipCarRecords.Include(x => x.Client!).ThenInclude(c => c.VipPoint)
            .FirstOrDefaultAsync(x => x.Vehicle == v);
    }

    public async Task<(bool ok, string msg)> CheckInAsync(CheckInVm vm)
    {
        var plate = Norm(vm.Vehicle);
        if (await _db.Cars.AnyAsync(c => c.Vehicle == plate && c.ReleaseTime == null))
            return (false, "This car is already in the parking lot.");
        int spaceId;
        if (vm.SpaceId.HasValue)
        {
            spaceId = vm.SpaceId.Value;
            if (await _db.Cars.AnyAsync(c => c.ParkingSpaceId == spaceId && c.ReleaseTime == null))
                return (false, $"Space {spaceId} is occupied.");
        }
        else
        {
            var taken = await _db.Cars.Where(c => c.ReleaseTime == null).Select(c => c.ParkingSpaceId).ToListAsync();
            spaceId = Enumerable.Range(1, 100).FirstOrDefault(i => !taken.Contains(i));
            if (spaceId == 0) return (false, "There are no available spaces.");
        }
        var vip = await FindVipAsync(plate);
        bool usePoint = vip?.Client?.VipPoint is { Points: > 0 };
        if (usePoint) vip!.Client!.VipPoint!.Points--;
        _db.Cars.Add(new Car
        {
            Vehicle = plate,
            Model = vm.Model.Trim(),
            EntryTime = DateTime.Now,
            ParkingSpaceId = spaceId,
            IsVip = vip != null,
            UsedPoint = usePoint
        });
        await _db.SaveChangesAsync();
        return (true, $"Registered at space {spaceId}." +
            (usePoint ? " VIP: 1 point used." : vip != null ? " VIP points exhausted – €1.20/hour." : ""));
    }

    public async Task<(bool ok, string msg)> ParkCarAsync(ParkCarViewModel vm, string ownerUserId)
    {
        var plate = Norm(vm.Vehicle);
        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        if (!await _db.ParkingSpaces.AnyAsync(space => space.Id == vm.SpaceId))
        {
            return (false, "The selected space does not exist.");
        }

        if (await _db.Cars.AnyAsync(car => car.Vehicle == plate && car.ReleaseTime == null))
        {
            return (false, "This car is already in the parking lot.");
        }

        if (await _db.Cars.AnyAsync(car => car.ParkingSpaceId == vm.SpaceId && car.ReleaseTime == null))
        {
            return (false, "The parking space is occupied; try another slot.");
        }

        var car = new Car
        {
            Vehicle = plate,
            Model = vm.Model.Trim(),
            EntryTime = DateTime.Now,
            ParkingSpaceId = vm.SpaceId,
            OwnerUserId = ownerUserId
        };
        _db.Cars.Add(car);

        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Your vehicle has been parked in space {vm.SpaceId}.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            _db.Entry(car).State = EntityState.Detached;
            if (await _db.Cars.AnyAsync(existing =>
                existing.ParkingSpaceId == vm.SpaceId && existing.ReleaseTime == null))
            {
                return (false, "The parking space is occupied; try another slot.");
            }

            if (await _db.Cars.AnyAsync(existing => existing.Vehicle == plate && existing.ReleaseTime == null))
            {
                return (false, "This car is already in the parking lot.");
            }

            throw;
        }
    }

    public async Task<(bool ok, string msg)> CheckOutAsync(int id, int? simMinutes)
    {
        if (simMinutes < 0) return (false, "Minutes must be non-negative.");
        var car = await _db.Cars.FindAsync(id);
        if (car == null || car.ReleaseTime != null) return (false, "Car not found.");
        car.ReleaseTime = simMinutes.HasValue ? car.EntryTime.AddMinutes(simMinutes.Value) : DateTime.Now;
        var min = (car.ReleaseTime.Value - car.EntryTime).TotalMinutes;
        car.Price = car.UsedPoint ? 0m
            : car.IsVip ? (min <= 15 ? 0m : (decimal)Math.Ceiling(min / 60) * 1.20m)
            : Price(min);
        await _db.SaveChangesAsync();
        return (true, $"Space {car.ParkingSpaceId} is free. Stay: {Math.Round(min)} min. Price: {car.Price:F2} €" +
               (car.UsedPoint ? " (paid with 1 VIP point)" : ""));
    }
}