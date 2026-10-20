using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class VipCreateVm
    {
        [Required(ErrorMessage = "Enter a first name.")]
        public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "Enter a last name.")]
        public string LastName { get; set; } = "";
        [Required(ErrorMessage = "Enter an address.")]
        public string Address { get; set; } = "";
        [Required(ErrorMessage = "Enter a city.")]
        public string City { get; set; } = "";
        [Required(ErrorMessage = "Enter a country.")]
        public string Country { get; set; } = "";
        [Required(ErrorMessage = "Enter a phone number."), RegularExpression(@"^\+?[0-9 ]{7,15}$", ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = "";
        [Required(ErrorMessage = "Enter an email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Enter a license plate."), RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Enter a valid license plate (4–12 letters or digits).")]
        public string Vehicle { get; set; } = "";
        [Required(ErrorMessage = "Enter a vehicle model.")]
        public string Model { get; set; } = "";
        [Required(ErrorMessage = "Enter the vehicle color.")]
        public string Color { get; set; } = "";
        [Range(1950, 2100, ErrorMessage = "Enter a valid year.")]
        public int Year { get; set; } = DateTime.Now.Year;



    }
}
