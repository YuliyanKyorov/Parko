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

    public static bool IsValidPlate(string? plate)
        => Regex.IsMatch(NormalizePlate(plate), @"^[A-Z\u0410-\u042F0-9]{4,12}$");

    public static decimal Price(double minutes, bool isVip = false)
        => ParkingPricing.Calculate(minutes, isVip);

    public async Task ResetPointsAsync()
    {
        var now = DateTime.Now;
        await _db.VipPoints
            .Where(point => point.LastReset.Year != now.Year || point.LastReset.Month != now.Month)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(point => point.Points, VipPoint.MonthlyPoints)
                .SetProperty(point => point.LastReset, now));
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
        if (model is null)
        {
            return (false, "Enter valid vehicle details.");
        }

        var plate = NormalizePlate(model.Vehicle);
        if (!IsValidPlate(plate) || string.IsNullOrWhiteSpace(model.Model) || model.Model.Trim().Length > 50)
        {
            return (false, "Enter valid vehicle details.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        if (await _db.Cars.AnyAsync(car => car.Vehicle == plate && car.ReleaseTime == null))
        {
            return (false, "This car is already in the parking lot.");
        }

        int spaceId;
        if (model.SpaceId.HasValue)
        {
            spaceId = model.SpaceId.Value;
            if (!await _db.ParkingSpaces.AnyAsync(space => space.Id == spaceId))
            {
                return (false, "The selected space does not exist.");
            }

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
        var usePoint = await TryUseVipPointAsync(vip?.Client?.VipPoint?.Id);

        var car = new Car
        {
            Vehicle = plate,
            Model = model.Model.Trim(),
            EntryTime = DateTime.Now,
            ParkingSpaceId = spaceId,
            IsVip = vip != null,
            UsedPoint = usePoint
        };

        _db.Cars.Add(car);
        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Registered at space {spaceId}." +
                (usePoint ? " VIP: 1 point used." : vip != null ? " VIP points exhausted – €1.20/hour." : ""));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            _db.Entry(car).State = EntityState.Detached;

            if (await _db.Cars.AnyAsync(existingCar =>
                existingCar.Vehicle == plate && existingCar.ReleaseTime == null))
            {
                return (false, "This car is already in the parking lot.");
            }

            if (await _db.Cars.AnyAsync(existingCar =>
                existingCar.ParkingSpaceId == spaceId && existingCar.ReleaseTime == null))
            {
                return (false, $"Space {spaceId} is occupied.");
            }

            throw;
        }
    }

    public async Task<(bool ok, string msg)> ParkCarAsync(ParkCarViewModel model, string ownerId)
    {
        if (model is null || string.IsNullOrWhiteSpace(ownerId))
        {
            return (false, "Enter valid vehicle details.");
        }

        var plate = NormalizePlate(model.Vehicle);
        if (!IsValidPlate(plate) || string.IsNullOrWhiteSpace(model.Model) || model.Model.Trim().Length > 50)
        {
            return (false, "Enter valid vehicle details.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

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

        var vip = await FindVipAsync(plate);
        var usePoint = await TryUseVipPointAsync(vip?.Client?.VipPoint?.Id);
        var car = new Car
        {
            Vehicle = plate,
            Model = model.Model.Trim(),
            EntryTime = DateTime.Now,
            ParkingSpaceId = model.SpaceId,
            OwnerUserId = ownerId,
            IsVip = vip != null,
            UsedPoint = usePoint
        };

        _db.Cars.Add(car);

        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Your vehicle has been parked in space {model.SpaceId}." +
                (usePoint ? " VIP: 1 point used." : vip != null ? " VIP points exhausted – VIP rates apply." : ""));
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
        if (id <= 0 || simMinutes < 0)
        {
            return (false, id <= 0 ? "Car not found." : "Minutes must be non-negative.");
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

    private async Task<bool> TryUseVipPointAsync(int? vipPointId)
    {
        if (!vipPointId.HasValue)
        {
            return false;
        }

        var updatedCount = await _db.VipPoints
            .Where(point => point.Id == vipPointId.Value && point.Points > 0)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(point => point.Points, point => point.Points - 1));

        return updatedCount == 1;
    }
}

public enum CarDeletionResult
{
    Deleted,
    NotFound,
    CarStillParked
}