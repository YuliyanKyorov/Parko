using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your username or email.")]
    [Display(Name = "Username or email")]
    public string Login { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [Display(Name = "Remember me for 30 days")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
