using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Parko.Models;
namespace Parko.Data;

public class ParkoDbContext : IdentityDbContext<ApplicationUser>
{
    public ParkoDbContext(DbContextOptions<ParkoDbContext> o) : base(o) { }
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<VipCarRecord> VipCarRecords => Set<VipCarRecord>();
    public DbSet<VipPoint> VipPoints => Set<VipPoint>();
    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();
    public DbSet<Car> Cars => Set<Car>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        base.OnModelCreating(m);
        m.Entity<ParkingSpace>().HasData(Enumerable.Range(1, 100).Select(i => new ParkingSpace { Id = i }));
        m.Entity<Client>().HasOne(c => c.VipPoint).WithOne().HasForeignKey<VipPoint>(p => p.ClientId);
        m.Entity<VipCarRecord>().HasIndex(v => v.Vehicle).IsUnique();
        m.Entity<Car>().Property(c => c.Price).HasColumnType("decimal(8,2)");
        m.Entity<Car>().HasOne(c => c.Owner).WithMany().HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);
        m.Entity<Car>().HasIndex(c => c.Vehicle).IsUnique().HasFilter("[ReleaseTime] IS NULL");
        m.Entity<Car>().HasIndex(c => c.ParkingSpaceId).IsUnique().HasFilter("[ReleaseTime] IS NULL");
    }
}
