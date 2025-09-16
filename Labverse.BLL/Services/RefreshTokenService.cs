using Labverse.BLL.DTOs.RefreshTokens;
using Labverse.BLL.Interfaces;
using Labverse.BLL.Settings;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Labverse.BLL.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<SaveRefreshTokenResponse> GenerateAndSaveAsync(int userId)
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
            UserId = userId,
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new SaveRefreshTokenResponse
        {
            Token = refreshToken.Token,
            Expires = refreshToken.Expires,
            UserId = refreshToken.UserId,
        };
    }

    public async Task<GetRefreshTokenResponse?> GetByTokenAsync(string token)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);

        if (refreshToken == null || !refreshToken.IsActive)
            return null;

        return new GetRefreshTokenResponse
        {
            Token = refreshToken.Token,
            Expires = refreshToken.Expires,
            IsRevoked = refreshToken.IsRevoked,
            IsUsed = refreshToken.IsUsed,
            UserId = refreshToken.UserId,
        };
    }

    public async Task MarkAsUsedAsync(string token)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);

        if (refreshToken == null)
            throw new KeyNotFoundException("Refresh token not found");

        refreshToken.IsUsed = true;

        _unitOfWork.RefreshTokens.Update(refreshToken);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RevokeAsync(string token)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);

        if (refreshToken == null)
            throw new KeyNotFoundException("Refresh token not found");

        refreshToken.IsRevoked = true;

        _unitOfWork.RefreshTokens.Update(refreshToken);
        await _unitOfWork.SaveChangesAsync();
    }
}
