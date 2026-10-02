using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class CheckInVm
    {
        [Required(ErrorMessage = "Въведи номер"), 
        RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Номер: 4–10 букви/цифри")] 
        public string Vehicle { get; set; } = "";

        [Required(ErrorMessage = "Въведи модел"), StringLength(50)] 
        public string Model { get; set; } = "";

        [Range(1, 100, ErrorMessage = "Място от 1 до 100")] 
        public int? SpaceId { get; set; }


    }
}
