namespace Labverse.BLL.DTOs.RefreshTokens;

public class SaveRefreshTokenResponse
{
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public int UserId { get; set; }
}
