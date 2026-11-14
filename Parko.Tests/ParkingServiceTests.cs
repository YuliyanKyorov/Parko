using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parko.Models;
using Parko.Services;

namespace Parko.Tests;

public class ParkingServiceTests
{
    [Fact]
    public async Task CheckIn_rejects_invalid_vehicle_and_unknown_space()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var service = new ParkingService(database.Context);

        var invalidVehicle = await service.CheckInAsync(new CheckInViewModel
        {
            Vehicle = "A--B",
            Model = "Car"
        });
        var unknownSpace = await service.CheckInAsync(new CheckInViewModel
        {
            Vehicle = "AB123",
            Model = "Car",
            SpaceId = 101
        });

        Assert.False(invalidVehicle.ok);
        Assert.False(unknownSpace.ok);
        Assert.Equal(0, await database.Context.Cars.CountAsync());
    }

    [Fact]
    public async Task CheckIn_rejects_duplicate_vehicle_and_occupied_space()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        database.Context.Cars.Add(NewCar("AB123", 1));
        await database.Context.SaveChangesAsync();
        var service = new ParkingService(database.Context);

        var duplicate = await service.CheckInAsync(new CheckInViewModel { Vehicle = "AB-123", Model = "Car" });
        var occupied = await service.CheckInAsync(new CheckInViewModel { Vehicle = "CD456", Model = "Car", SpaceId = 1 });

        Assert.False(duplicate.ok);
        Assert.Contains("already", duplicate.msg);
        Assert.False(occupied.ok);
        Assert.Contains("occupied", occupied.msg);
    }

    [Fact]
    public async Task CheckIn_assigns_first_available_space_and_uses_refreshed_vip_point()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        database.Context.Cars.Add(NewCar("OLD1", 1));
        var vip = await AddVipAsync(database.Context, "AB123", 2, DateTime.Now.AddMonths(-1));
        await database.Context.SaveChangesAsync();
        var service = new ParkingService(database.Context);

        var result = await service.CheckInAsync(new CheckInViewModel
        {
            Vehicle = "ab-123",
            Model = " Sedan "
        });

        var car = await database.Context.Cars.SingleAsync(existing => existing.Vehicle == "AB123");
        Assert.True(result.ok);
        Assert.Equal(2, car.ParkingSpaceId);
        Assert.True(car.IsVip);
        Assert.True(car.UsedPoint);
        Assert.Equal(VipPoint.MonthlyPoints - 1, await database.Context.VipPoints
            .AsNoTracking()
            .Where(points => points.ClientId == vip.Id)
            .Select(points => points.Points)
            .SingleAsync());
        Assert.Contains("1 point used", result.msg);
    }

    [Fact]
    public async Task CheckIn_reports_full_lot_and_vip_without_points()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        for (var spaceId = 1; spaceId <= 100; spaceId++)
        {
            database.Context.Cars.Add(NewCar($"CAR{spaceId:0000}", spaceId));
        }

        await database.Context.SaveChangesAsync();
        var noSpace = await new ParkingService(database.Context).CheckInAsync(
            new CheckInViewModel { Vehicle = "NEW123", Model = "Car" });
        Assert.False(noSpace.ok);
        Assert.Contains("no available", noSpace.msg);

        await database.Context.Cars.ExecuteDeleteAsync();
        await AddVipAsync(database.Context, "NEW123", 0, DateTime.Now);
        var vipWithoutPoints = await new ParkingService(database.Context).CheckInAsync(
            new CheckInViewModel { Vehicle = "NEW123", Model = "Car" });

        Assert.True(vipWithoutPoints.ok);
        Assert.Contains("points exhausted", vipWithoutPoints.msg);
        Assert.False((await database.Context.Cars.SingleAsync()).UsedPoint);
    }

    [Fact]
    public async Task ParkCar_validates_space_and_prevents_duplicate_or_occupied_parking()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        database.Context.Users.Add(new ApplicationUser { Id = "owner" });
        database.Context.Cars.Add(NewCar("AB123", 1));
        await database.Context.SaveChangesAsync();
        var service = new ParkingService(database.Context);

        var invalidSpace = await service.ParkCarAsync(
            new ParkCarViewModel { SpaceId = 101, Vehicle = "CD456", Model = "Car" }, "owner");
        var duplicate = await service.ParkCarAsync(
            new ParkCarViewModel { SpaceId = 2, Vehicle = "AB-123", Model = "Car" }, "owner");
        var occupied = await service.ParkCarAsync(
            new ParkCarViewModel { SpaceId = 1, Vehicle = "CD456", Model = "Car" }, "owner");

        Assert.False(invalidSpace.ok);
        Assert.False(duplicate.ok);
        Assert.False(occupied.ok);
        Assert.Equal(1, await database.Context.Cars.CountAsync());
    }

    [Fact]
    public async Task ParkCar_saves_owner_and_validates_required_service_arguments()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        database.Context.Users.Add(new ApplicationUser { Id = "owner" });
        await database.Context.SaveChangesAsync();
        var service = new ParkingService(database.Context);

        var invalidInput = await service.ParkCarAsync(new ParkCarViewModel
        {
            SpaceId = 1,
            Vehicle = "AB123",
            Model = new string('x', 51)
        }, "owner");
        var invalidOwner = await service.ParkCarAsync(
            new ParkCarViewModel { SpaceId = 1, Vehicle = "AB123", Model = "Car" }, " ");
        var parked = await service.ParkCarAsync(
            new ParkCarViewModel { SpaceId = 1, Vehicle = "AB123", Model = "Car" }, "owner");

        Assert.False(invalidInput.ok);
        Assert.False(invalidOwner.ok);
        Assert.True(parked.ok);
        var car = await database.Context.Cars.SingleAsync();
        Assert.Equal("owner", car.OwnerUserId);
        Assert.Equal(1, car.ParkingSpaceId);
    }

    [Fact]
    public async Task FindVip_resets_points_and_matches_normalized_plate()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var vip = await AddVipAsync(database.Context, "AB123", 3, DateTime.Now.AddMonths(-1));

        var found = await new ParkingService(database.Context).FindVipAsync("ab-123");

        Assert.NotNull(found);
        Assert.Equal("AB123", found.Vehicle);
        Assert.Equal(VipPoint.MonthlyPoints, await database.Context.VipPoints
            .AsNoTracking()
            .Where(points => points.ClientId == vip.Id)
            .Select(points => points.Points)
            .SingleAsync());
        Assert.Null(await new ParkingService(database.Context).FindVipAsync("CD456"));
    }

    [Fact]
    public async Task CheckOut_rejects_invalid_ids_and_duration_and_missing_or_released_cars()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var service = new ParkingService(database.Context);

        Assert.False((await service.CheckOutAsync(0, null)).ok);
        Assert.False((await service.CheckOutAsync(99, null)).ok);
        Assert.False((await service.CheckOutAsync(1, -1)).ok);

        var released = NewCar("AB123", 1);
        released.ReleaseTime = DateTime.Now;
        database.Context.Cars.Add(released);
        await database.Context.SaveChangesAsync();
        Assert.False((await service.CheckOutAsync(released.Id, null)).ok);
    }

    [Fact]
    public async Task CheckOut_calculates_price_and_honors_vip_point_payment()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var standard = NewCar("AB123", 1);
        var vip = NewCar("CD456", 2);
        vip.IsVip = true;
        var free = NewCar("EF789", 3);
        free.UsedPoint = true;
        database.Context.Cars.AddRange(standard, vip, free);
        await database.Context.SaveChangesAsync();
        var service = new ParkingService(database.Context);

        var standardResult = await service.CheckOutAsync(standard.Id, 75);
        var vipResult = await service.CheckOutAsync(vip.Id, 76);
        var freeResult = await service.CheckOutAsync(free.Id, 60);

        Assert.True(standardResult.ok);
        Assert.Equal(1.20m, standard.Price);
        Assert.Equal(2.40m, vip.Price);
        Assert.Equal(0m, free.Price);
        Assert.Contains("paid with 1 VIP point", freeResult.msg);
    }

    [Fact]
    public async Task DeleteReleasedCar_distinguishes_missing_active_and_released_records()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var parked = NewCar("AB123", 1);
        var released = NewCar("CD456", 2);
        released.ReleaseTime = DateTime.Now;
        database.Context.Cars.AddRange(parked, released);
        await database.Context.SaveChangesAsync();
        var service = new ParkingService(database.Context);

        Assert.Equal(CarDeletionResult.NotFound, await service.DeleteReleasedCarAsync(999));
        Assert.Equal(CarDeletionResult.CarStillParked, await service.DeleteReleasedCarAsync(parked.Id));
        Assert.Equal(CarDeletionResult.Deleted, await service.DeleteReleasedCarAsync(released.Id));
        Assert.False(await database.Context.Cars.AnyAsync(car => car.Id == released.Id));
    }

    private static Car NewCar(string plate, int spaceId) => new()
    {
        Vehicle = plate,
        Model = "Test car",
        EntryTime = DateTime.Now,
        ParkingSpaceId = spaceId
    };

    private static async Task<Client> AddVipAsync(
        Parko.Data.ParkoDbContext context,
        string plate,
        int points,
        DateTime lastReset)
    {
        var client = new Client
        {
            FirstName = "Test",
            LastName = "Client",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            Phone = "+35912345678",
            Email = $"{plate}@example.test",
            VipPoint = new VipPoint { Points = points, LastReset = lastReset }
        };
        client.VipCarRecords.Add(new VipCarRecord
        {
            Vehicle = plate,
            Model = "Test car",
            Color = "Blue",
            Year = 2020
        });
        context.Clients.Add(client);
        await context.SaveChangesAsync();
        return client;
    }
}
