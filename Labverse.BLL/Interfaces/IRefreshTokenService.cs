using Labverse.BLL.DTOs.RefreshTokens;

namespace Labverse.BLL.Interfaces;

public interface IRefreshTokenService
{
    Task<SaveRefreshTokenResponse> GenerateAndSaveAsync(int userId);
    Task<GetRefreshTokenResponse?> GetByTokenAsync(string token);
    Task MarkAsUsedAsync(string token);
    Task RevokeAsync(string token);
}
