namespace Labverse.BLL.DTOs.Users;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public AuthUserDto User { get; set; } = null!;
}
