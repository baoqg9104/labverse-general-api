namespace Labverse.BLL.DTOs.Users;

public class ExternalAuthRequestDto
{
    public string IdToken { get; set; } = string.Empty; // Firebase ID token received from client
}
