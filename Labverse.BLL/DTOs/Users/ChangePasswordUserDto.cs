using System.ComponentModel.DataAnnotations;

namespace Labverse.BLL.DTOs.Users;

public class ChangePasswordUserDto
{
    [Required(ErrorMessage = "Password is required.")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?""{}|<>]).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character."
    )]
    public string NewPassword { get; set; } = string.Empty;
}
