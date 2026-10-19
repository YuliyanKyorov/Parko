using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Въведете потребителско име или имейл.")]
    [Display(Name = "Потребителско име или имейл")]
    public string Login { get; set; } = "";

    [Required(ErrorMessage = "Въведете парола.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = "";

    [Display(Name = "Запомни ме за 30 дни")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
