using Labverse.DAL.EntitiesModels;

namespace Labverse.DAL.Repositories.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}
