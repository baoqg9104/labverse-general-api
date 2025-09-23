using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRecaptchaService _recaptchaService;

    public UsersController(
        IUserService userService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IRecaptchaService recaptchaService
    )
    {
        _userService = userService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _recaptchaService = recaptchaService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    // Get me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();
        if (!int.TryParse(userId, out var userIdInt))
            return Unauthorized();
        var user = await _userService.GetByIdAsync(userIdInt);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        await _userService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = await _userService.AddAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthRequestDto dto)
    {
        // Verify reCAPTCHA
        var isRecaptchaValid = await _recaptchaService.VerifyTokenAsync(dto.RecaptchaToken);

        if (!isRecaptchaValid)
            return BadRequest("Invalid reCAPTCHA");

        // Authenticate user
        var user = await _userService.Authenticate(dto);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var accessToken = _jwtService.GenerateAccessToken(user);

        var refreshToken = await _refreshTokenService.GenerateAndSaveAsync(user.Id);

        Response.Cookies.Append(
            "refreshToken",
            refreshToken.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshToken.Expires,
            }
        );

        var response = new AuthResponseDto { AccessToken = accessToken };

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshTokenCookie = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshTokenCookie))
        {
            // Revoke refresh token
            await _refreshTokenService.RevokeAsync(refreshTokenCookie);

            // Delete refresh token cookie
            Response.Cookies.Append(
                "refreshToken",
                "",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Domain = "localhost",
                }
            );
        }

        return Ok(new { message = "Logged out successfully" });
    }
}
