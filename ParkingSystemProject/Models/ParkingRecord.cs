namespace ParkingSystemProject.Models
{
    public class ParkingRecord
    {        
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public Vehicle Vehicle { get; set; } = null!;

        public int ParkingSpaceId { get; set; }

        public ParkingSpace ParkingSpace { get; set; } = null!;

        public DateTime EntryTime { get; set; } = DateTime.Now;

        public DateTime? ExitTime { get; set; }

        public int? DurationMinutes { get; set; }

        public decimal Price { get; set; }

        public bool UsedVipPoint { get; set; }

        public bool IsPaid { get; set; }
    }
}

