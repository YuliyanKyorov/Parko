using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class VipCarRecordEditViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }

    [Required, RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$",
        ErrorMessage = "Enter a valid license plate (4–12 letters or digits).")]
    public string Vehicle { get; set; } = "";

    [Required, StringLength(50)]
    public string Model { get; set; } = "";

    [Required, StringLength(30)]
    public string Color { get; set; } = "";

    [Range(1950, 2100, ErrorMessage = "Enter a valid year.")]
    public int Year { get; set; } = DateTime.Now.Year;
}
