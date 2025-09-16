using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/refresh-tokens")]
[ApiController]
[Authorize]
public class RefreshTokensController : ControllerBase
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public RefreshTokensController(IRefreshTokenService refreshTokenService, IUserService userService, IJwtService jwtService)
    {
        _refreshTokenService = refreshTokenService;
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetByToken(string token)
    {
        var refreshToken = await _refreshTokenService.GetByTokenAsync(token);
        if (refreshToken == null) return NotFound();
        return Ok(refreshToken);
    }

    [HttpPatch("{token}/used")]
    public async Task<IActionResult> MarkAsUsed(string token)
    {
        await _refreshTokenService.MarkAsUsedAsync(token);
        return NoContent();
    }

    [HttpPatch("{token}/revoke")]
    public async Task<IActionResult> Revoke(string token)
    {
        await _refreshTokenService.RevokeAsync(token);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(string token)
    {
        var refreshToken = await _refreshTokenService.GetByTokenAsync(token);

        if (refreshToken == null) return Unauthorized("Invalid or expired refresh token");

        await _refreshTokenService.MarkAsUsedAsync(token);

        var user = await _userService.GetByIdAsync(refreshToken.UserId);

        if (user == null)
            return Unauthorized();

        var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Username, user.Role);

        var newRefreshToken = await _refreshTokenService.GenerateAndSaveAsync(user.Id);

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            User = new AuthUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            }
        };

        return Ok(response);
    }
}
