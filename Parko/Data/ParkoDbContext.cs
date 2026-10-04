using Microsoft.EntityFrameworkCore;
using Parko.Models;
namespace Parko.Data;

public class ParkoDbContext : DbContext
{
    public ParkoDbContext(DbContextOptions<ParkoDbContext> o) : base(o) { }
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<VipCarRecord> VipCarRecords => Set<VipCarRecord>();
    public DbSet<VipPoint> VipPoints => Set<VipPoint>();
    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();
    public DbSet<Car> Cars => Set<Car>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<ParkingSpace>().HasData(Enumerable.Range(1, 100).Select(i => new ParkingSpace { Id = i }));
        m.Entity<Client>().HasOne(c => c.VipPoint).WithOne().HasForeignKey<VipPoint>(p => p.ClientId);
        m.Entity<VipCarRecord>().HasIndex(v => v.Vehicle).IsUnique();
        m.Entity<Car>().Property(c => c.Price).HasColumnType("decimal(8,2)");
    }
}
