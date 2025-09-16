namespace Labverse.DAL.EntitiesModels;

public class Badge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;

    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
