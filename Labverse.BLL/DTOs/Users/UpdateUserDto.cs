using System.ComponentModel.DataAnnotations;

namespace Labverse.BLL.DTOs.Users;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?""{}|<>]).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character."
    )]
    public string Password { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(20, ErrorMessage = "Username cannot exceed 20 characters")]
    public string Username { get; set; }

    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
}
