using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class VipCreateViewModel
    {
        [Required(ErrorMessage = "Enter a first name."), StringLength(50)]
        public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "Enter a last name."), StringLength(50)]
        public string LastName { get; set; } = "";
        [Required(ErrorMessage = "Enter an address."), StringLength(120)]
        public string Address { get; set; } = "";
        [Required(ErrorMessage = "Enter a city."), StringLength(60)]
        public string City { get; set; } = "";
        [Required(ErrorMessage = "Enter a country."), StringLength(60)]
        public string Country { get; set; } = "";
        [Required(ErrorMessage = "Enter a phone number."), StringLength(16), RegularExpression(@"^\+?[0-9 ]{7,15}$", ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = "";
        [Required(ErrorMessage = "Enter an email address."), StringLength(254), EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Enter a license plate."), StringLength(12, MinimumLength = 4)]
        [RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Enter a valid license plate (4–12 letters or digits).")]
        public string Vehicle { get; set; } = "";
        [Required(ErrorMessage = "Enter a vehicle model."), StringLength(50)]
        public string Model { get; set; } = "";
        [Required(ErrorMessage = "Enter the vehicle color."), StringLength(30)]
        public string Color { get; set; } = "";
        [Range(1950, 2100, ErrorMessage = "Enter a valid year.")]
        public int Year { get; set; } = DateTime.Now.Year;



    }
}
