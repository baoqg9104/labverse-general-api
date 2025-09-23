using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/refresh-tokens")]
[ApiController]
public class RefreshTokensController : ControllerBase
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public RefreshTokensController(
        IRefreshTokenService refreshTokenService,
        IUserService userService,
        IJwtService jwtService
    )
    {
        _refreshTokenService = refreshTokenService;
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetByToken(string token)
    {
        var refreshToken = await _refreshTokenService.GetByTokenAsync(token);
        if (refreshToken == null)
            return NotFound();
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
    public async Task<IActionResult> Refresh()
    {
        // Get refresh token from cookies
        var refreshTokenCookie = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshTokenCookie))
            return Unauthorized("Refresh token not found");

        // Validate refresh token
        var refreshToken = await _refreshTokenService.GetByTokenAsync(refreshTokenCookie);

        if (refreshToken == null)
            return Unauthorized("Invalid or expired refresh token");

        // Mark the old refresh token as used
        await _refreshTokenService.MarkAsUsedAsync(refreshTokenCookie);

        // Get the user associated with the refresh token
        var user = await _userService.GetByIdAsync(refreshToken.UserId);

        if (user == null)
            return Unauthorized("User not found");

        // Generate new access token and refresh token
        var newAccessToken = _jwtService.GenerateAccessToken(user);

        var newRefreshToken = await _refreshTokenService.GenerateAndSaveAsync(user.Id);

        // Set the new refresh token in cookies
        Response.Cookies.Append(
            "refreshToken",
            newRefreshToken.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshToken.Expires,
            }
        );

        var response = new AuthResponseDto { AccessToken = newAccessToken };

        return Ok(response);
    }
}
