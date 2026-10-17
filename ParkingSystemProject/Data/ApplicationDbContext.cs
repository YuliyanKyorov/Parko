using Microsoft.EntityFrameworkCore;
using ParkingSystemProject.Models;

namespace ParkingSystemProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(
                @"Server=DESKTOP-6IHPPVM\SQLEXPRESS;Database=MyDatabase;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<VipCustomer> VipCustomers { get; set; }

        public DbSet<ParkingSpace> ParkingSpaces { get; set; }

        public DbSet<ParkingRecord> ParkingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<ParkingSpace>()
                .HasIndex(p => p.SpaceNumber)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.VipCustomer)
                .WithMany(v => v.Vehicles)
                .HasForeignKey(v => v.VipCustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ParkingRecord>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.ParkingRecords)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingRecord>()
                .HasOne(p => p.ParkingSpace)
                .WithMany(s => s.ParkingRecords)
                .HasForeignKey(p => p.ParkingSpaceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingRecord>()
                .Property(p => p.Price)
                .HasPrecision(10, 2);
        }
    }
}

