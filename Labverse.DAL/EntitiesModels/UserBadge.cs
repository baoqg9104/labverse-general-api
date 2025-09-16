namespace Labverse.DAL.EntitiesModels;

public class UserBadge : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;
    public DateTime DateAwarded { get; set; }
}
