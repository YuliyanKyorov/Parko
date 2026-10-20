using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class CheckInVm
    {
        [Required(ErrorMessage = "Enter a license plate."),
        RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Enter a valid license plate (4–12 letters or digits).")]
        public string Vehicle { get; set; } = "";

        [Required(ErrorMessage = "Enter model"), StringLength(50)] 
        public string Model { get; set; } = "";

        [Range(1, 100, ErrorMessage = "Space must be between 1 and 100.")]
        public int? SpaceId { get; set; }


    }
}
