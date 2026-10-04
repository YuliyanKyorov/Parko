using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class CheckInVm
    {
        [Required(ErrorMessage = "Enter number"), 
        RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Number: 4–10 letters/digits")] 
        public string Vehicle { get; set; } = "";

        [Required(ErrorMessage = "Enter model"), StringLength(50)] 
        public string Model { get; set; } = "";

        [Range(1, 100, ErrorMessage = "Space from 1 to 100")] 
        public int? SpaceId { get; set; }


    }
}
