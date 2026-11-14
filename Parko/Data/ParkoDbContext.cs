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
            modelBuilder.Entity<ParkingSpace>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_ParkingSpaces_Id_Range", "[Id] >= 1 AND [Id] <= 100"));
            modelBuilder.Entity<VipPoint>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_VipPoints_Points_Range", "[Points] >= 0 AND [Points] <= 40"));
            modelBuilder.Entity<VipCarRecord>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_VipCarRecords_Year_Range", "[Year] >= 1950 AND [Year] <= 2100"));
            modelBuilder.Entity<Client>()
                .ToTable(table =>
                {
                    table.HasCheckConstraint(
                        "CK_Clients_Phone_Length",
                        "DATALENGTH([Phone]) >= 2 AND DATALENGTH([Phone]) <= 32");
                    table.HasCheckConstraint(
                        "CK_Clients_Email_Length",
                        "DATALENGTH([Email]) >= 2 AND DATALENGTH([Email]) <= 508");
                });
            modelBuilder.Entity<Car>()
                .ToTable(table =>
                {
                    table.HasCheckConstraint(
                        "CK_Cars_Vehicle_Length",
                        "DATALENGTH([Vehicle]) >= 8 AND DATALENGTH([Vehicle]) <= 24");
                    table.HasCheckConstraint(
                        "CK_Cars_Model_Length",
                        "DATALENGTH([Model]) >= 2 AND DATALENGTH([Model]) <= 100");
                    table.HasCheckConstraint(
                        "CK_Cars_Price_NonNegative",
                        "[Price] IS NULL OR [Price] >= 0");
                });
            modelBuilder.Entity<VipCarRecord>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_VipCarRecords_Vehicle_Length",
                    "DATALENGTH([Vehicle]) >= 8 AND DATALENGTH([Vehicle]) <= 24"));
            
            modelBuilder.Entity<Car>().HasOne(c => c.Owner).WithMany()
                .HasForeignKey(c => c.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            
            modelBuilder.Entity<Car>().HasIndex(c => c.Vehicle).IsUnique().HasFilter("[ReleaseTime] IS NULL");
            modelBuilder.Entity<Car>().HasIndex(c => c.ParkingSpaceId).IsUnique().HasFilter("[ReleaseTime] IS NULL");
        }
    }
}