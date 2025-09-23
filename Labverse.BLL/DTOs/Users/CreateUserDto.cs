using System.ComponentModel.DataAnnotations;

namespace Labverse.BLL.DTOs.Users;

public class CreateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(20, ErrorMessage = "Username cannot exceed 20 characters")]
    public string Username { get; set; }

    public string RecaptchaToken { get; set; }

}
