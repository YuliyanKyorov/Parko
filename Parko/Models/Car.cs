using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class Car
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^[A-Z\u0410-\u042F0-9]{4,12}$")]
        public string Vehicle { get; set; } = "";
        [Required]
        public string Model { get; set; } = "";
        public DateTime EntryTime { get; set; }
        public DateTime? ReleaseTime { get; set; }
        public decimal? Price { get; set; }
        public int ParkingSpaceId { get; set; }
        public ParkingSpace? ParkingSpace { get; set; }
        public string? OwnerUserId { get; set; }
        public ApplicationUser? Owner { get; set; }
        public bool IsVip { get; set; }
        public bool UsedPoint { get; set; }

    }
}