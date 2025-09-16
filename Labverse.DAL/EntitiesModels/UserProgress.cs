namespace Labverse.DAL.EntitiesModels;

public enum ProgressStatus
{
    NotStarted,
    InProgress,
    Completed
}

public class UserProgress : BaseEntity
{
    public ProgressStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int UserId { get; set; }
    public int LabId { get; set; }

    public User User { get; set; } = null!;
    public Lab Lab { get; set; } = null!;
}
