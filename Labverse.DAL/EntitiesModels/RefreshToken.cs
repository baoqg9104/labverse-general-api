using System.ComponentModel.DataAnnotations.Schema;

namespace Labverse.DAL.EntitiesModels;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsExpired => DateTime.UtcNow >= Expires;

    [NotMapped]
    public override bool IsActive => !IsRevoked && !IsUsed && !IsExpired;
}
