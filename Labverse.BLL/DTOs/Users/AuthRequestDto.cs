using System.ComponentModel.DataAnnotations;

namespace Labverse.BLL.DTOs.Users;

public class AuthRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }

    public string RecaptchaToken { get; set; }
}
