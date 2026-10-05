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

    // Редовна тарифа: до 15 мин безплатно; до 6 ч 1,20 €/ч; 6–12 ч 1 €/ч; над 12 ч 12 €/ден
    public static decimal Price(double min) =>
        min <= 15 ? 0m :
        min <= 360 ? (decimal)Math.Ceiling(min / 60) * 1.20m :
        min <= 720 ? (decimal)Math.Ceiling(min / 60) * 1.00m :
        (decimal)Math.Ceiling(min / 1440) * 12m;

    // На 1-во число точките стават 40, неизползваните се губят
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
            return (false, "This car is already in the parking lot.");// Проверud дали автомобилът вече е паркиран.
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
            if (spaceId == 0) return (false, "There are no free spaces.");// Няма свободни места.
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
            (usePoint ? " VIP: 1 point reported." : vip != null ? " VIP points exhausted – 1.20 €/hour." : ""));// Регистрация на място. VIP: 1 точка отчетена  или са изчерпани – 1,20 €/час.
    }

    public async Task<(bool ok, string msg)> CheckOutAsync(int id, int? simMinutes)
    {
        if (simMinutes < 0) return (false, "Minutes must be non-negative.");
        var car = await _db.Cars.FindAsync(id);
        if (car == null || car.ReleaseTime != null) return (false, "Car not found.");// Колата не е намерена.
        car.ReleaseTime = simMinutes.HasValue ? car.EntryTime.AddMinutes(simMinutes.Value) : DateTime.Now;
        var min = (car.ReleaseTime.Value - car.EntryTime).TotalMinutes;
        car.Price = car.UsedPoint ? 0m
            : car.IsVip ? (min <= 15 ? 0m : (decimal)Math.Ceiling(min / 60) * 1.20m)
            : Price(min);
        await _db.SaveChangesAsync();
        return (true, $"Space {car.ParkingSpaceId} is free. Stay: {Math.Round(min)} min. Price: {car.Price:F2} €" +
               (car.UsedPoint ? " (paid with 1 VIP point)" : ""));// Мястото е свободно. Престой: минути. Цена: евро (платено с 1 VIP точка)
    }
}
/* ВНИМАНИЕ: Да проверя дали има нужда от метод за извеждане на текущите заети места и свободните места, както и списък с всички коли в паркинга.
Не съм сигурен дали това е нужно, но може да е полезно за администраторите на паркинга.
САМО КОТО ИДЕЯ ВЪПРОСИ: Какво точно искат да виждат администраторите?
Може би списък с всички коли, заедно с информация за тях (номер, модел, време на влизане, време на излизане, цена и т.н.) и текущото състояние
на паркинга (колко места са заети и колко са свободни).*/