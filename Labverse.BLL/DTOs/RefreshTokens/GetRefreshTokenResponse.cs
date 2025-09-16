namespace Labverse.BLL.DTOs.RefreshTokens;

public class GetRefreshTokenResponse
{
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public int UserId { get; set; }
}
