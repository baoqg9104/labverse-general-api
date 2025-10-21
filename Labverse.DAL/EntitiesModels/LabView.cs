namespace Labverse.DAL.EntitiesModels;

public class LabView : BaseEntity
{
    public int LabId { get; set; }
    public int? UserId { get; set; } // null for anonymous
    public string? Ip { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public Lab Lab { get; set; } = null!;
    public User? User { get; set; }
}
