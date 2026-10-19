using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class ParkCarViewModel
{
    [Required(ErrorMessage = "Изберете свободно място.")]
    [Range(1, 100, ErrorMessage = "Мястото трябва да е от 1 до 100.")]
    [Display(Name = "Свободно място")]
    public int SpaceId { get; set; }

    [Required(ErrorMessage = "Въведете регистрационен номер.")]
    [RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Въведете валиден регистрационен номер.")]
    [Display(Name = "Регистрационен номер")]
    public string Vehicle { get; set; } = "";

    [Required(ErrorMessage = "Въведете модел на автомобила.")]
    [StringLength(50)]
    [Display(Name = "Модел")]
    public string Model { get; set; } = "";
}
