using System.ComponentModel.DataAnnotations;

namespace Labverse.BLL.DTOs.Users;

public class CreateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?""{}|<>]).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character."
    )]
    public string Password { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(
        100,
        MinimumLength = 4,
        ErrorMessage = "Username must be between 4 and 100 characters"
    )]
    //[RegularExpression(
    //    "^[A-Za-z][A-Za-z0-9_ ]{3,99}$",
    //    ErrorMessage = "Username must start with a letter and contain only letters, numbers, spaces, and underscores"
    //)]
    public string Username { get; set; }

    public string RecaptchaToken { get; set; }
}
