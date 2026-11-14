using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Parko.Models;

namespace Parko.Data
{
    public class ParkoDbContext : IdentityDbContext<ApplicationUser>
    {
        public ParkoDbContext(DbContextOptions<ParkoDbContext> options) : base(options) { }
        
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<VipCarRecord> VipCarRecords => Set<VipCarRecord>();
        public DbSet<VipPoint> VipPoints => Set<VipPoint>();
        public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();
        public DbSet<Car> Cars => Set<Car>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            
            for (var spaceId = 1; spaceId <= 100; spaceId++)
            {
                modelBuilder.Entity<ParkingSpace>().HasData(new ParkingSpace { Id = spaceId });
            }
            
           
            modelBuilder.Entity<Client>().HasOne(c => c.VipPoint).WithOne().HasForeignKey<VipPoint>(p => p.ClientId);
            modelBuilder.Entity<VipCarRecord>().HasIndex(v => v.Vehicle).IsUnique();
            modelBuilder.Entity<Car>().Property(c => c.Price).HasColumnType("decimal(8,2)");
            
           
            modelBuilder.Entity<Car>().HasOne(c => c.Owner).WithMany().HasForeignKey(c => c.OwnerUserId);
            
            
            modelBuilder.Entity<Car>().HasIndex(c => c.Vehicle).IsUnique().HasFilter("[ReleaseTime] IS NULL");
            modelBuilder.Entity<Car>().HasIndex(c => c.ParkingSpaceId).IsUnique().HasFilter("[ReleaseTime] IS NULL");
        }
    }
}