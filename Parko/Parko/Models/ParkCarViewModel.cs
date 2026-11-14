using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class ParkCarViewModel
{
    [Required(ErrorMessage = "Select an available space.")]
    [Range(1, 100, ErrorMessage = "Space must be between 1 and 100.")]
    [Display(Name = "Available space")]
    public int SpaceId { get; set; }

    [Required(ErrorMessage = "Enter a license plate.")]
    [RegularExpression(@"^[A-ZА-Яa-zа-я0-9 \-]{4,12}$", ErrorMessage = "Enter a valid license plate.")]
    [Display(Name = "License plate")]
    public string Vehicle { get; set; } = "";

    [Required(ErrorMessage = "Enter the vehicle model.")]
    [StringLength(50)]
    [Display(Name = "Model")]
    public string Model { get; set; } = "";
}
