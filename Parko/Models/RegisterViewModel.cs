using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Enter a username.")]
    [StringLength(256, ErrorMessage = "Username cannot be longer than 256 characters.")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Enter an email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Enter a password.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
}
