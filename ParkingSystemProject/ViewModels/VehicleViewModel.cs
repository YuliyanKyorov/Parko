using System.ComponentModel.DataAnnotations;

namespace ParkingSystemProject.ViewModels
{
    public class VehicleViewModel
    {       
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int ProductionYear { get; set; }

        [MaxLength(50)]
        public string Color { get; set; } = string.Empty;

        public bool IsVip { get; set; }

        public int? VipCustomerId { get; set; }
    }
}