using System.ComponentModel.DataAnnotations;

namespace ParkingSystemProject.ViewModels
{
    public class ParkingSpaceViewModel
    { 
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string SpaceNumber { get; set; } = string.Empty;

        public bool IsOccupied { get; set; }
    }
}