using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class CloseAccountViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string Password { get; set; } = "";
}
