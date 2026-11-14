using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Parko.Data;
using Parko.Models;
namespace Parko.Services;

public class ParkingService
{
    private readonly ParkoDbContext _db;

    public ParkingService(ParkoDbContext db)
    {
        _db = db;
    }

    public static string NormalizePlate(string? plate)
        => Regex.Replace(plate ?? string.Empty, @"[\s-]", string.Empty).ToUpperInvariant();

    public static decimal Price(double minutes, bool isVip = false)
        => ParkingPricing.Calculate(minutes, isVip);

    public async Task ResetPointsAsync()
    {
        var now = DateTime.Now;
        var points = await _db.VipPoints
            .Where(point => point.LastReset.Year != now.Year || point.LastReset.Month != now.Month)
            .ToListAsync();

        foreach (var point in points)
        {
            point.Points = VipPoint.MonthlyPoints;
            point.LastReset = now;
        }

        if (points.Count > 0)
        {
            await _db.SaveChangesAsync();
        }
    }

    public async Task<VipCarRecord?> FindVipAsync(string plate)
    {
        await ResetPointsAsync();
        var normalizedPlate = NormalizePlate(plate);

        return await _db.VipCarRecords
            .Include(vipRecord => vipRecord.Client!)
            .ThenInclude(client => client.VipPoint)
            .FirstOrDefaultAsync(vipRecord => vipRecord.Vehicle == normalizedPlate);
    }

    public async Task<(bool ok, string msg)> CheckInAsync(CheckInViewModel model)
    {
        var plate = NormalizePlate(model.Vehicle);

        if (await _db.Cars.AnyAsync(car => car.Vehicle == plate && car.ReleaseTime == null))
        {
            return (false, "This car is already in the parking lot.");
        }

        int spaceId;
        if (model.SpaceId.HasValue)
        {
            spaceId = model.SpaceId.Value;
            if (await _db.Cars.AnyAsync(car => car.ParkingSpaceId == spaceId && car.ReleaseTime == null))
            {
                return (false, $"Space {spaceId} is occupied.");
            }
        }
        else
        {
            var occupiedIds = await _db.Cars
                .Where(car => car.ReleaseTime == null)
                .Select(car => car.ParkingSpaceId)
                .ToListAsync();

            spaceId = Enumerable.Range(1, 100).FirstOrDefault(id => !occupiedIds.Contains(id));
            if (spaceId == 0)
            {
                return (false, "There are no available spaces.");
            }
        }

        var vip = await FindVipAsync(plate);
        var usePoint = vip?.Client?.VipPoint is { Points: > 0 };

        if (usePoint)
        {
            vip!.Client!.VipPoint!.Points--;
        }

        _db.Cars.Add(new Car
        {
            Vehicle = plate,
            Model = model.Model.Trim(),
            EntryTime = DateTime.Now,
            ParkingSpaceId = spaceId,
            IsVip = vip != null,
            UsedPoint = usePoint
        });

        await _db.SaveChangesAsync();
        return (true, $"Registered at space {spaceId}." +
            (usePoint ? " VIP: 1 point used." : vip != null ? " VIP points exhausted – €1.20/hour." : ""));
    }

    public async Task<(bool ok, string msg)> ParkCarAsync(ParkCarViewModel model, string ownerId)
    {
        var plate = NormalizePlate(model.Vehicle);

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        if (!await _db.ParkingSpaces.AnyAsync(space => space.Id == model.SpaceId))
        {
            return (false, "The selected space does not exist.");
        }

        if (await _db.Cars.AnyAsync(car => car.Vehicle == plate && car.ReleaseTime == null))
        {
            return (false, "This car is already in the parking lot.");
        }

        if (await _db.Cars.AnyAsync(car => car.ParkingSpaceId == model.SpaceId && car.ReleaseTime == null))
        {
            return (false, "The parking space is occupied; try another slot.");
        }

        var car = new Car
        {
            Vehicle = plate,
            Model = model.Model.Trim(),
            EntryTime = DateTime.Now,
            ParkingSpaceId = model.SpaceId,
            OwnerUserId = ownerId
        };

        _db.Cars.Add(car);

        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Your vehicle has been parked in space {model.SpaceId}.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            _db.Entry(car).State = EntityState.Detached;

            if (await _db.Cars.AnyAsync(existingCar =>
                existingCar.ParkingSpaceId == model.SpaceId && existingCar.ReleaseTime == null))
            {
                return (false, "The parking space is occupied; try another slot.");
            }

            if (await _db.Cars.AnyAsync(existingCar => existingCar.Vehicle == plate && existingCar.ReleaseTime == null))
            {
                return (false, "This car is already in the parking lot.");
            }

            throw;
        }
    }

    public async Task<(bool ok, string msg)> CheckOutAsync(int id, int? simMinutes)
    {
        if (simMinutes < 0)
        {
            return (false, "Minutes must be non-negative.");
        }

        var car = await _db.Cars.FindAsync(id);
        if (car == null || car.ReleaseTime != null)
        {
            return (false, "Car not found.");
        }

        car.ReleaseTime = simMinutes.HasValue ? car.EntryTime.AddMinutes(simMinutes.Value) : DateTime.Now;
        var minutes = (car.ReleaseTime.Value - car.EntryTime).TotalMinutes;
        var price = car.UsedPoint ? 0m : Price(minutes, car.IsVip);

        car.Price = price;
        await _db.SaveChangesAsync();

        return (true, $"Space {car.ParkingSpaceId} is free. Stay: {Math.Round(minutes)} min. Price: {car.Price:F2} €" +
               (car.UsedPoint ? " (paid with 1 VIP point)" : ""));
    }

    public async Task<CarDeletionResult> DeleteReleasedCarAsync(int id)
    {
        var car = await _db.Cars.FindAsync(id);
        if (car == null)
        {
            return CarDeletionResult.NotFound;
        }

        if (car.ReleaseTime == null)
        {
            return CarDeletionResult.CarStillParked;
        }

        _db.Cars.Remove(car);
        await _db.SaveChangesAsync();
        return CarDeletionResult.Deleted;
    }
}

public enum CarDeletionResult
{
    Deleted,
    NotFound,
    CarStillParked
}