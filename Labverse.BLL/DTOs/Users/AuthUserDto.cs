using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Users;

public class AuthUserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public UserRole Role { get; set; }
}
