using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class VipCarRecord
    {
        public int Id { get; set; }
        [Required, RegularExpression(@"^[A-ZА-Я0-9]{4,10}$", ErrorMessage = "Номер: 4–10 букви/цифри")]
        public string Vehicle { get; set; } = "";
        [Required, StringLength(50)] 
        public string Model { get; set; } = "";
        [Required, StringLength(30)] 
        public string Color { get; set; } = "";
        [Range(1950, 2100, ErrorMessage = "Невалидна година")]
        public int Year { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }

    }
}
