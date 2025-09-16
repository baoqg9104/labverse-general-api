using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(int userId, string email, string username, UserRole role);
}
