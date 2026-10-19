using System.ComponentModel.DataAnnotations;

namespace Parko.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Въведете потребителско име.")]
    [StringLength(256, ErrorMessage = "Потребителското име не може да е по-дълго от 256 символа.")]
    [Display(Name = "Потребителско име")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Въведете имейл адрес.")]
    [EmailAddress(ErrorMessage = "Въведете валиден имейл адрес.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Въведете парола.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Паролата трябва да е поне 8 символа.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Потвърдете паролата.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат.")]
    [Display(Name = "Потвърдете паролата")]
    public string ConfirmPassword { get; set; } = "";
}
