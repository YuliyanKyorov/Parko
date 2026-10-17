using System.ComponentModel.DataAnnotations;

namespace ParkingSystemProject.Models
{
    public class ParkingSpace
    { 
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string SpaceNumber { get; set; } = string.Empty;

        public bool IsOccupied { get; set; }

        public ICollection<ParkingRecord> ParkingRecords { get; set; } = new List<ParkingRecord>();
    }
}

