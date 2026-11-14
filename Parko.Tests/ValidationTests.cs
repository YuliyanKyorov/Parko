using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Parko.Models;

namespace Parko.Tests;

public class ValidationTests
{
    [Fact]
    public void Parking_and_vip_view_models_reject_invalid_values()
    {
        Assert.False(IsValid(new CheckInViewModel { Vehicle = "ABC", Model = "Car", SpaceId = 101 }));
        Assert.False(IsValid(new ParkCarViewModel { SpaceId = 0, Vehicle = "AB123", Model = "Car" }));
        Assert.False(IsValid(new VipCreateViewModel
        {
            FirstName = new string('x', 51),
            LastName = "Client",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            Phone = "+35912345678",
            Email = "client@example.test",
            Vehicle = "AB123",
            Model = "Car",
            Color = "Blue",
            Year = 2020
        }));
        Assert.False(IsValid(new VipClientEditViewModel { Points = VipPoint.MonthlyPoints + 1 }));
    }

    [Fact]
    public async Task Database_constraints_reject_out_of_range_spaces_points_and_prices()
    {
        await using var database = await SqliteParkoDatabase.CreateAsync();
        var context = database.Context;

        var invalidClient = new Client
        {
            FirstName = "Test",
            LastName = "Client",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            Phone = new string('1', 17),
            Email = "client@example.test"
        };
        context.Clients.Add(invalidClient);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        context.Entry(invalidClient).State = EntityState.Detached;

        context.ParkingSpaces.Add(new ParkingSpace { Id = 101 });
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        context.Entry(context.ParkingSpaces.Local.Single(space => space.Id == 101)).State = EntityState.Detached;

        var client = new Client
        {
            FirstName = "Test",
            LastName = "Client",
            Address = "1 Main Street",
            City = "Sofia",
            Country = "Bulgaria",
            Phone = "+35912345678",
            Email = "client@example.test"
        };
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        var invalidPoints = new VipPoint { ClientId = client.Id, Points = VipPoint.MonthlyPoints + 1 };
        context.VipPoints.Add(invalidPoints);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        context.Entry(invalidPoints).State = EntityState.Detached;

        var invalidModel = new Car
        {
            Vehicle = "AB123",
            Model = new string('x', 51),
            EntryTime = DateTime.Now,
            ParkingSpaceId = 1
        };
        context.Cars.Add(invalidModel);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        context.Entry(invalidModel).State = EntityState.Detached;

        var invalidPrice = new Car
        {
            Vehicle = "AB123",
            Model = "Test car",
            EntryTime = DateTime.Now,
            ReleaseTime = DateTime.Now,
            Price = -1,
            ParkingSpaceId = 1
        };
        context.Cars.Add(invalidPrice);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static bool IsValid(object model)
    {
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);
    }
}
