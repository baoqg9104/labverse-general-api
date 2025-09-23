using Labverse.BLL.DTOs.Users;

namespace Labverse.BLL.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(UserDto user);
}
