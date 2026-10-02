using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class VipCreateVm
    {
        [Required] 
        public string FirstName { get; set; } = "";
        [Required] 
        public string LastName { get; set; } = "";
        [Required] 
        public string Address { get; set; } = "";
        [Required] 
        public string City { get; set; } = "";
        [Required] 
        public string Country { get; set; } = "";
        [Required, RegularExpression(@"^\+?[0-9 ]{7,15}$", ErrorMessage = "Невалиден телефон")] 
        public string Phone { get; set; } = "";
        [Required, EmailAddress] 
        public string Email { get; set; } = "";
        [Required, RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Номер: 4–10 букви/цифри")] 
        public string Vehicle { get; set; } = "";
        [Required] 
        public string Model { get; set; } = "";
        [Required] 
        public string Color { get; set; } = "";
        [Range(1950, 2100, ErrorMessage = "Невалидна година")] 
        public int Year { get; set; } = DateTime.Now.Year;



    }
}
