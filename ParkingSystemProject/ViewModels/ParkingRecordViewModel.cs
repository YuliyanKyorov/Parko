using System.ComponentModel.DataAnnotations;

namespace ParkingSystemProject.ViewModels
{
    public class ParkingRecordViewModel
    {        
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public string? VehicleRegistrationNumber { get; set; }

        public int ParkingSpaceId { get; set; }

        public string? ParkingSpaceNumber { get; set; }

        public DateTime EntryTime { get; set; }

        public DateTime? ExitTime { get; set; }

        public int? DurationMinutes { get; set; }

        public decimal Price { get; set; }

        public bool UsedVipPoint { get; set; }

        public bool IsPaid { get; set; }
    }
}