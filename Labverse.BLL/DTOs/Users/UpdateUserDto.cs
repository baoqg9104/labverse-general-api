using System.ComponentModel.DataAnnotations;

namespace Labverse.BLL.DTOs.Users;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(20, ErrorMessage = "Username cannot exceed 20 characters")]
    public string Username { get; set; }

    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
}
