using Labverse.API.Helpers;
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
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IConfiguration _configuration;

    public UsersController(
        IUserService userService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IRecaptchaService recaptchaService,
        IEmailVerificationService emailVerificationService,
        IConfiguration configuration
    )
    {
        _userService = userService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _recaptchaService = recaptchaService;
        _emailVerificationService = emailVerificationService;
        _configuration = configuration;
    }

    [HttpGet]
    //[Authorize]
    public async Task<IActionResult> GetUsers([FromQuery] bool? isOnlyVerifiedUser = false, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var users = await _userService.GetAllAsync(isOnlyVerifiedUser, includeInactive);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_USERS_ERROR", ex.Message, 500);
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return ApiErrorHelper.Error("USER_NOT_FOUND", "User not found", 404);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_USER_ERROR", ex.Message, 500);
        }
    }

    // Get me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);
            if (!int.TryParse(userId, out var userIdInt))
                return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);
            var user = await _userService.GetByIdAsync(userIdInt);
            if (user == null)
                return ApiErrorHelper.Error("USER_NOT_FOUND", "User not found", 404);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_ME_ERROR", ex.Message, 500);
        }
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

            if (!int.TryParse(userId, out var userIdInt))
                return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

            if (userIdInt != id)
                return ApiErrorHelper.Error(
                    "FORBIDDEN",
                    "You can only update your own profile",
                    403
                );

            await _userService.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return ApiErrorHelper.Error("USER_NOT_FOUND", "User not found", 404);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("UPDATE_USER_ERROR", ex.Message, 500);
        }
    }

    [HttpPatch("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordUserDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

            if (!int.TryParse(userId, out var userIdInt))
                return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

            await _userService.ChangePasswordAsync(userIdInt, dto);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return ApiErrorHelper.Error("USER_NOT_FOUND", "User not found", 404);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiErrorHelper.Error("INVALID_OLD_PASSWORD", ex.Message, 400);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CHANGE_PASSWORD_ERROR", ex.Message, 500);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userService.AddAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrorHelper.Error("EMAIL_EXISTS", ex.Message, 400);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CREATE_USER_ERROR", ex.Message, 500);
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return ApiErrorHelper.Error("USER_NOT_FOUND", "User not found", 404);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("DELETE_USER_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("{id}/restore")]
    [Authorize]
    public async Task<IActionResult> RestoreUser(int id)
    {
        try
        {
            await _userService.RestoreAsync(id);
            return Ok(new { message = "User restored" });
        }
        catch (KeyNotFoundException)
        {
            return ApiErrorHelper.Error("USER_NOT_FOUND", "User not found", 404);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("RESTORE_USER_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthRequestDto dto)
    {
        var isRecaptchaValid = await _recaptchaService.VerifyTokenAsync(dto.RecaptchaToken);
        if (!isRecaptchaValid)
            return ApiErrorHelper.Error("INVALID_RECAPTCHA", "Invalid reCAPTCHA", 400);
        try
        {
            var user = await _userService.Authenticate(dto);
            if (user == null)
                return ApiErrorHelper.Error("INVALID_CREDENTIALS", "Invalid credentials", 401);

            if (user.EmailVerifiedAt == null)
            {
                await _emailVerificationService.SendVerificationEmailAsync(user.Id, user.Email);
                return ApiErrorHelper.Error("EMAIL_NOT_VERIFIED", "Email not verified.", 401);
            }

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
        catch (InvalidOperationException ex)
            when (ex.Message == "Account exists but email is not verified")
        {
            return ApiErrorHelper.Error(
                "EMAIL_NOT_VERIFIED",
                "Account exists but email is not verified",
                401
            );
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("AUTHENTICATE_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var refreshTokenCookie = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshTokenCookie))
            {
                await _refreshTokenService.RevokeAsync(refreshTokenCookie);
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
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("LOGOUT_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] CreateUserDto dto)
    {
        var isRecaptchaValid = await _recaptchaService.VerifyTokenAsync(dto.RecaptchaToken);
        if (!isRecaptchaValid)
            return ApiErrorHelper.Error("INVALID_RECAPTCHA", "Invalid reCAPTCHA", 400);
        try
        {
            var user = await _userService.AddAsync(dto);
            await _emailVerificationService.SendVerificationEmailAsync(user.Id, user.Email);
            return Ok(
                new
                {
                    message = "Signup successful. Please check your email to verify your account.",
                }
            );
        }
        catch (InvalidOperationException ex)
        {
            return ApiErrorHelper.Error("EMAIL_EXISTS", ex.Message, 400);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("SIGNUP_ERROR", ex.Message, 500);
        }
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        string frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "https://localhost:5173";
        string verifyResultPath = "/verify-result";
        string frontendUrl = frontendBaseUrl.TrimEnd('/') + verifyResultPath;
        try
        {
            var decodedToken = Uri.UnescapeDataString(token.Replace(" ", "+"));
            var result = await _emailVerificationService.VerifyTokenAsync(decodedToken);
            var status = result ? "success" : "error";
            var message = result ? "Email verified successfully." : "Invalid or expired token.";
            var redirectUrl =
                $"{frontendUrl}?status={status}&message={Uri.EscapeDataString(message)}";
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            var redirectUrl =
                $"{frontendUrl}?status=error&message={Uri.EscapeDataString(ex.Message)}";
            return Redirect(redirectUrl);
        }
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] string email)
    {
        try
        {
            var user = (await _userService.GetAllAsync()).FirstOrDefault(u => u.Email == email);
            if (user == null)
                return ApiErrorHelper.Error("USER_NOT_FOUND", "User not found", 404);
            if (user.EmailVerifiedAt != null)
                return ApiErrorHelper.Error(
                    "EMAIL_ALREADY_VERIFIED",
                    "Email already verified",
                    400
                );
            await _emailVerificationService.SendVerificationEmailAsync(user.Id, user.Email);
            return Ok(new { message = "Verification email resent successfully." });
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("RESEND_VERIFICATION_ERROR", ex.Message, 500);
        }
    }
}
