namespace Labverse.DAL.EntitiesModels;

public class ActivityHistory : BaseEntity
{
    public int UserId { get; set; }
    public int? LabId { get; set; }
    public int? QuestionId { get; set; }
    public string Action { get; set; } = string.Empty; // e.g., answer_submitted, xp_awarded, streak_bonus, lab_completed, level_up, badge_awarded
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }

    public User User { get; set; } = null!;
    public Lab? Lab { get; set; }
    public LabQuestion? Question { get; set; }
}
