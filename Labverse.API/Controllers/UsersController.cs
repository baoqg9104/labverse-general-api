using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public UsersController(IUserService userService, IJwtService jwtService, IRefreshTokenService refreshTokenService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
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
        if (user == null) return NotFound();
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
        var user = await _userService.Authenticate(dto);
        if (user == null) return Unauthorized("Invalid credentials");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Username, user.Role);
        var refreshToken = await _refreshTokenService.GenerateAndSaveAsync(user.Id);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
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
