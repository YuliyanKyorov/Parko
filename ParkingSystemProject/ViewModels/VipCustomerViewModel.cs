using System.ComponentModel.DataAnnotations;

namespace ParkingSystemProject.ViewModels
{
    public class VipCustomerViewModel
    {      
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public int Points { get; set; } = 40;

        public DateTime PointsResetDate { get; set; }
    }
}