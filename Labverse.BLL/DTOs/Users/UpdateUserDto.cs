using System.ComponentModel.DataAnnotations;

namespace Labverse.BLL.DTOs.Users;

public class UpdateUserDto
{
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

    public string? AvatarUrl { get; set; } = null;
    public string? Bio { get; set; } = null;
}
