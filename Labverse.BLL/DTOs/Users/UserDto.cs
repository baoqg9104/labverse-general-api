using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
