namespace Labverse.DAL.EntitiesModels;

public class LabComment : BaseEntity
{
    public int LabId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentId { get; set; }

    public Lab Lab { get; set; } = null!;
    public User User { get; set; } = null!;

    // Navigation for tree
    public LabComment? Parent { get; set; }
    public ICollection<LabComment> Replies { get; set; } = new List<LabComment>();
}
