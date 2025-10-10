namespace Labverse.DAL.EntitiesModels;

public class UserLabAnswer : BaseEntity
{
    public int UserId { get; set; }
    public int LabId { get; set; }
    public int QuestionId { get; set; }
    public string AnswerJson { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public User User { get; set; } = null!;
    public Lab Lab { get; set; } = null!;
    public LabQuestion Question { get; set; } = null!;
}
